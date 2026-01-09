using RateLimiter.Models;

namespace RateLimiter.Rules;

public sealed class RateLimiterOptions
{
    public RouteRule DefaultRule { get; init; } = new(60, TimeSpan.FromMinutes(1));

    public IReadOnlyDictionary<string, RouteRule> RouteOverrides { get; init; }
        = new Dictionary<string, RouteRule>(StringComparer.OrdinalIgnoreCase);

    public TimeSpan IdleTtl { get; init; } = TimeSpan.FromMinutes(10);

    public int CleanupInterval { get; init; } = 1000;
}
