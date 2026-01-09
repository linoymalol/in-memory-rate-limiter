using System.Collections.Concurrent;
using System.Threading;
using RateLimiter.Models;
using RateLimiter.Rules;

namespace RateLimiter.Core;

public sealed class InMemoryRateLimiter : IRateLimiter
{
    private readonly RateLimiterOptions _options;
    private readonly ConcurrentDictionary<RateLimitKey, RateLimitState> _states = new();

    private readonly long _idleTtlTicks;
    private readonly int _cleanupInterval;
    private long _callCount;

    public InMemoryRateLimiter(RateLimiterOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));

        _idleTtlTicks = options.IdleTtl <= TimeSpan.Zero ? 0 : options.IdleTtl.Ticks;
        _cleanupInterval = options.CleanupInterval;
    }

    public AllowResult Allow(string identity, string route, DateTimeOffset now)
    {
        if (identity is null)
            throw new ArgumentNullException(nameof(identity));
        if (route is null)
            throw new ArgumentNullException(nameof(route));

        if (string.IsNullOrWhiteSpace(identity))
            throw new ArgumentException("Identity must not be empty or whitespace.", nameof(identity));
        if (string.IsNullOrWhiteSpace(route))
            throw new ArgumentException("Route must not be empty or whitespace.", nameof(route));

        var rule = _options.RouteOverrides.TryGetValue(route, out var overrideRule)
            ? overrideRule
            : _options.DefaultRule;

        var nowTicks = now.UtcTicks;
        var windowTicks = rule.Window.Ticks;
        var cutoffTicks = nowTicks - windowTicks;

        var key = new RateLimitKey(identity, route);
        var state = _states.GetOrAdd(key, _ => new RateLimitState());

        AllowResult result;

        lock (state.SyncRoot)
        {
            Volatile.Write(ref state.LastSeenUtcTicks, nowTicks);

            while (state.AllowedTicks.Count > 0 && state.AllowedTicks.Peek() <= cutoffTicks)
            {
                state.AllowedTicks.Dequeue();
            }

            if (state.AllowedTicks.Count >= rule.MaxRequests)
            {
                var oldestTicks = state.AllowedTicks.Peek();
                var waitTicks = (oldestTicks + windowTicks) - nowTicks;
                if (waitTicks < 0) waitTicks = 0;

                result = new AllowResult(false, TimeSpan.FromTicks(waitTicks), 0);
            }
            else
            {
                state.AllowedTicks.Enqueue(nowTicks);
                var remaining = rule.MaxRequests - state.AllowedTicks.Count;
                result = new AllowResult(true, null, remaining);
            }
        }

        if (ShouldCleanup())
        {
            CleanupStaleStates(nowTicks);
        }

        return result;
    }

    private bool ShouldCleanup()
    {
        if (_cleanupInterval <= 0 || _idleTtlTicks <= 0)
            return false;

        var count = Interlocked.Increment(ref _callCount);
        return count % _cleanupInterval == 0;
    }

    private void CleanupStaleStates(long nowTicks)
    {
        foreach (var entry in _states)
        {
            var state = entry.Value;
            var lastSeen = Volatile.Read(ref state.LastSeenUtcTicks);

            if (lastSeen <= nowTicks - _idleTtlTicks)
            {
                _states.TryRemove(entry.Key, out _);
            }
        }
    }
}
