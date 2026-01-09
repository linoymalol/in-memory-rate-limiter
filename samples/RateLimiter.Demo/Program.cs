using RateLimiter.Core;
using RateLimiter.Rules;

var limiter = new InMemoryRateLimiter(new RateLimiterOptions());

for (int i = 1; i <= 65; i++)
{
    var result = limiter.Allow("user1", "/orders", DateTimeOffset.UtcNow);
    Console.WriteLine($"Request {i}: Allowed={result.Allowed}, RetryAfter={result.RetryAfter}");
}
