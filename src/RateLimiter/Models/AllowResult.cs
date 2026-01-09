namespace RateLimiter.Models;

public sealed record AllowResult(bool Allowed, TimeSpan? RetryAfter, int Remaining);
