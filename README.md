# In-Memory Rate Limiter

A lightweight, in-memory rate limiter for .NET applications. It tracks requests per identity/route pair, enforces configurable windows, and supports route-specific overrides.

## Features

- Sliding window enforcement per identity and route.
- Per-route override rules.
- Idle TTL eviction with lazy cleanup (no background threads).

## Build and Run

This repository contains a class library plus a small demo application under `samples/`.

```bash
dotnet build
dotnet run --project samples/RateLimiter.Demo/RateLimiter.Demo.csproj
```

## Usage

```csharp
using RateLimiter.Core;
using RateLimiter.Models;
using RateLimiter.Rules;

var options = new RateLimiterOptions
{
    DefaultRule = new RouteRule(60, TimeSpan.FromMinutes(1)),
    RouteOverrides = new Dictionary<string, RouteRule>
    {
        ["/login"] = new RouteRule(5, TimeSpan.FromMinutes(1))
    },
    IdleTtl = TimeSpan.FromMinutes(10),
    CleanupInterval = 1000
};

var limiter = new InMemoryRateLimiter(options);

var result = limiter.Allow("user-123", "/login", DateTimeOffset.UtcNow);
if (!result.Allowed)
{
    Console.WriteLine($"Retry after {result.RetryAfter}");
}
```

## Run Tests

If you have the .NET SDK installed, run:

```bash
dotnet test
```

## Algorithm Choice - Sliding Window Log

 For each (identity, route) the limiter stores timestamps of allowed requests in a FIFO queue.
 On each call:
 - Remove timestamps older than the window.
 - If the count is at the limit → reject and compute RetryAfter from the oldest timestamp.
 - Otherwise → allow and enqueue the current timestamp.
Tradeoff: this is precise but stores one entry per allowed request in the active window.

## Concurrency Strategy

 - Per-key state is stored in a ConcurrentDictionary.
 - Each key has its own lock (SyncRoot) to ensure correct pruning/enqueue under concurrency.
 - Cleanup triggering uses Interlocked.Increment (no global lock).

## Memory Cleanup Strategy

To prevent unbounded growth, the limiter supports idle eviction:
 - Each key tracks LastSeenUtcTicks.
 - Every CleanupInterval calls, the limiter scans keys and removes entries idle for more than IdleTtl.
 - Cleanup is best-effort and runs outside per-key locks.

## Tradeoffs and Limitations

 - In-memory only (not shared across processes/servers).
 - Cleanup is O(n) over tracked keys and may cause occasional spikes with many keys.
 - No async API (caller supplies DateTimeOffset, which simplifies deterministic testing).

 ## With more time, improvements would include:

 - Add a distributed implementation (e.g., Redis) for multi-instance rate limiting.
 - Add metrics hooks (allowed/denied counts, cleanup duration, tracked keys).
 - Support alternative algorithms (token bucket / sliding window counter) for lower memory use.
