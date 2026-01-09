namespace RateLimiter.Models;

public sealed record RouteRule(int MaxRequests, TimeSpan Window);
