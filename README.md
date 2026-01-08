# In-Memory Rate Limiter

## Build & Run

```bash
dotnet build
```

```bash
dotnet run --project RateLimiter.Demo
```

## Run Tests

```bash
dotnet test
```

## API

- `IRateLimiter.Allow(identity, route, now)`
- `RateLimiterOptions` (default rule + per-route overrides)
- `AllowResult` (allowed + retry-after + remaining)
- `RouteRule`

## Algorithm choice

_TODO: describe sliding window log strategy._

## Concurrency strategy

_TODO: describe thread-safety approach._

## Memory cleanup strategy

_TODO: describe cleanup/eviction plan._

## Tradeoffs / Improvements

_TODO: list tradeoffs and future improvements._
