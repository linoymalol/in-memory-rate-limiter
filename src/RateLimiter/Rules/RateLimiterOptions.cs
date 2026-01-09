using RateLimiter.Models;

namespace RateLimiter.Rules;

public sealed class RateLimiterOptions
{
    public required RouteRule DefaultRule { get; init; } = new(60, TimeSpan.FromMinutes(1));

    public IReadOnlyDictionary<string, RouteRule> RouteOverrides { get; init; }
        = new Dictionary<string, RouteRule>(StringComparer.OrdinalIgnoreCase);
}
