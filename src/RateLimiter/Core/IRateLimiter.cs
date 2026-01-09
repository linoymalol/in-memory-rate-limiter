using RateLimiter.Models;

namespace RateLimiter.Core;

public interface IRateLimiter
{
    AllowResult Allow(string identity, string route, DateTimeOffset now);
}
