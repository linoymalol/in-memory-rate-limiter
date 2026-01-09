using System.Collections.Generic;

namespace RateLimiter.Core;

internal sealed class RateLimitState
{
    public object SyncRoot { get; } = new();

    public Queue<long> AllowedTicks { get; } = new();
}
