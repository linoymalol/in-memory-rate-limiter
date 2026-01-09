using RateLimiter.Core;
using RateLimiter.Rules;
using RateLimiter.Models;
using Xunit;

namespace RateLimiter.Tests;

public class InMemoryRateLimiterTests
{
    [Fact]
    public void Allow_RespectsMaxRequestsAndReportsRemaining()
    {
        var options = new RateLimiterOptions
        {
            DefaultRule = new RouteRule(2, TimeSpan.FromMinutes(1))
        };
        var limiter = new InMemoryRateLimiter(options);
        var now = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        var first = limiter.Allow("user", "/", now);
        var second = limiter.Allow("user", "/", now);
        var third = limiter.Allow("user", "/", now);

        Assert.True(first.Allowed);
        Assert.Equal(1, first.Remaining);
        Assert.True(second.Allowed);
        Assert.Equal(0, second.Remaining);
        Assert.False(third.Allowed);
        Assert.Equal(TimeSpan.FromMinutes(1), third.RetryAfter);
    }

    [Fact]
    public void Allow_ResetsAfterWindow()
    {
        var options = new RateLimiterOptions
        {
            DefaultRule = new RouteRule(1, TimeSpan.FromSeconds(10))
        };
        var limiter = new InMemoryRateLimiter(options);
        var now = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        var first = limiter.Allow("user", "/", now);
        var second = limiter.Allow("user", "/", now.AddSeconds(11));

        Assert.True(first.Allowed);
        Assert.True(second.Allowed);
        Assert.Equal(0, second.Remaining);
    }

    [Fact]
    public void Allow_UsesRouteOverrides()
    {
        var options = new RateLimiterOptions
        {
            DefaultRule = new RouteRule(1, TimeSpan.FromMinutes(1)),
            RouteOverrides = new Dictionary<string, RouteRule>
            {
                ["/fast"] = new RouteRule(2, TimeSpan.FromMinutes(1))
            }
        };
        var limiter = new InMemoryRateLimiter(options);
        var now = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        var first = limiter.Allow("user", "/fast", now);
        var second = limiter.Allow("user", "/fast", now);

        Assert.True(first.Allowed);
        Assert.True(second.Allowed);
        Assert.Equal(0, second.Remaining);
    }

    [Fact]
    public void Allow_CleansUpIdleStates()
    {
        var options = new RateLimiterOptions
        {
            DefaultRule = new RouteRule(5, TimeSpan.FromHours(1)),
            IdleTtl = TimeSpan.FromMinutes(1),
            CleanupInterval = 1
        };
        var limiter = new InMemoryRateLimiter(options);
        var start = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        limiter.Allow("old", "/", start);

        var later = start.AddMinutes(2);
        limiter.Allow("new", "/", later);
        var refreshed = limiter.Allow("old", "/", later);

        Assert.True(refreshed.Allowed);
        Assert.Equal(4, refreshed.Remaining);
    }
}
