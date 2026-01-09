using System.Collections.Concurrent;
using RateLimiter.Models;
using RateLimiter.Rules;

namespace RateLimiter.Core;

public sealed class InMemoryRateLimiter : IRateLimiter
{
    private readonly RateLimiterOptions _options;
    private readonly ConcurrentDictionary<RateLimitKey, RateLimitState> _states = new();

    public InMemoryRateLimiter(RateLimiterOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public AllowResult Allow(string identity, string route, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(identity))
        {
            throw new ArgumentNullException(nameof(identity));
        }

        if (string.IsNullOrWhiteSpace(route))
        {
            throw new ArgumentNullException(nameof(route));
        }

        var rule = _options.RouteOverrides.TryGetValue(route, out var overrideRule)
            ? overrideRule
            : _options.DefaultRule;
        var key = new RateLimitKey(identity, route);
        var state = _states.GetOrAdd(key, _ => new RateLimitState());

        var nowTicks = now.UtcTicks;
        var windowTicks = rule.Window.Ticks;
        var cutoffTicks = nowTicks - windowTicks;

        lock (state.SyncRoot)
        {
            while (state.AllowedTicks.Count > 0 && state.AllowedTicks.Peek() <= cutoffTicks)
            {
                state.AllowedTicks.Dequeue();
            }

            if (state.AllowedTicks.Count >= rule.MaxRequests)
            {
                var oldestTicks = state.AllowedTicks.Peek();
                var waitTicks = (oldestTicks + windowTicks) - nowTicks;
                if (waitTicks < 0) waitTicks = 0;

                return new AllowResult(false, TimeSpan.FromTicks(waitTicks), 0);
            }

            state.AllowedTicks.Enqueue(nowTicks);

            var remaining = rule.MaxRequests - state.AllowedTicks.Count;
            return new AllowResult(true, null, remaining);
        }

    }
}
