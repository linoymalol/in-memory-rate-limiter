using RateLimiter.Models;

namespace RateLimiter.Rules;

public sealed class RateLimiterOptions
{
    public RouteRule DefaultRule { get; init; } = new(1, TimeSpan.FromSeconds(1));

    public IReadOnlyDictionary<string, RouteRule> RouteOverrides { get; init; }
        = new Dictionary<string, RouteRule>(StringComparer.OrdinalIgnoreCase);
}
