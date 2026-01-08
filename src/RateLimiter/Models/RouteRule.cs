namespace RateLimiter.Models;

public sealed record RouteRule(string Route, int PermitLimit, TimeSpan Window);
