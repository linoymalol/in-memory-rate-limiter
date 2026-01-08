using RateLimiter.Models;
using RateLimiter.Rules;

namespace RateLimiter.Core;

public sealed class InMemoryRateLimiter : IRateLimiter
{
    private readonly RateLimiterOptions _options;

    public InMemoryRateLimiter(RateLimiterOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public AllowResult Allow(string identity, string route, DateTimeOffset now)
    {
        throw new NotImplementedException("Rate limiting logic not implemented yet.");
    }
}
