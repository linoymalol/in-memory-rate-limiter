using RateLimiter.Core;
using RateLimiter.Rules;
using RateLimiter.Models;

var options = new RateLimiterOptions
{
    DefaultRule = new RouteRule(60, TimeSpan.FromMinutes(1)),
    RouteOverrides = new Dictionary<string, RouteRule>(StringComparer.OrdinalIgnoreCase)
    {
        ["/login"] = new RouteRule(10, TimeSpan.FromSeconds(10))
    }
};

var limiter = new InMemoryRateLimiter(options);


for (int i = 1; i <= 65; i++)
{
    var result = limiter.Allow("user1", "/orders", DateTimeOffset.UtcNow);
    Console.WriteLine($"Request {i}: Allowed={result.Allowed}, RetryAfter={result.RetryAfter}");
}
