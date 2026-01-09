namespace RateLimiter.Models;

public readonly record struct RateLimitKey(string Identity, string Route);
