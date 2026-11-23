namespace BusinessLogic;

/// <summary>
/// Per-process counters: used when Redis is not configured, and while it is unreachable.
/// </summary>
public sealed class InMemoryRateLimiter : IRateLimiter
{
    private const int PurgeEveryHits = 1000;

    private readonly object m_lock = new();
    private readonly Dictionary<(string Policy, string Subject), Counter> m_counters = new();
    private readonly TimeProvider m_timeProvider;
    private int m_hitsSincePurge;

    public InMemoryRateLimiter( TimeProvider timeProvider )
    {
        m_timeProvider = timeProvider;
    }

    public Task<bool> TryAcquireAsync( RateLimitPolicy policy, string subject )
    {
        DateTimeOffset now = m_timeProvider.GetUtcNow();
        lock (m_lock)
        {
            PurgeExpired( now );

            (string, string) key = (policy.Name, subject);
            if (!m_counters.TryGetValue( key, out Counter? counter ) || counter.ExpiresAt <= now)
            {
                counter = new Counter( now + policy.Window );
                m_counters[key] = counter;
            }

            counter.Hits++;
            return Task.FromResult( counter.Hits <= policy.Limit );
        }
    }

    public Task ResetAsync( RateLimitPolicy policy, string subject )
    {
        lock (m_lock)
        {
            m_counters.Remove( (policy.Name, subject) );
        }

        return Task.CompletedTask;
    }

    private void PurgeExpired( DateTimeOffset now )
    {
        if (++m_hitsSincePurge < PurgeEveryHits)
        {
            return;
        }

        m_hitsSincePurge = 0;
        foreach ((string, string) key in m_counters.Where( c => c.Value.ExpiresAt <= now ).Select( c => c.Key ).ToList())
        {
            m_counters.Remove( key );
        }
    }

    private sealed class Counter
    {
        public Counter( DateTimeOffset expiresAt )
        {
            ExpiresAt = expiresAt;
        }

        public DateTimeOffset ExpiresAt { get; }
        public int Hits { get; set; }
    }
}
