using RateLimiter.Core;
using RateLimiter.Rules;

var limiter = new InMemoryRateLimiter(new RateLimiterOptions());

Console.WriteLine($"Rate limiter demo placeholder: {limiter.GetType().Name}");
