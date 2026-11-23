using System.Security.Cryptography;
using System.Text;
using System.Threading;

using StackExchange.Redis;

namespace BusinessLogic;

/// <summary>
/// Counters shared by all API instances that survive restarts. While Redis is unreachable it
/// counts in process instead, so an outage neither blocks sign-in nor removes the limits.
/// </summary>
public sealed class RedisRateLimiter : IRateLimiter
{
    /// <summary>INCR and expiry in one step, so a crash between them cannot leave a key without a TTL.</summary>
    private const string HitScript =
        "local hits = redis.call('INCR', KEYS[1]) " +
        "if hits == 1 then redis.call('PEXPIRE', KEYS[1], ARGV[1]) end " +
        "return hits";

    private readonly IConnectionMultiplexer m_redis;
    private readonly InMemoryRateLimiter m_fallback;
    private int m_isDown;

    public RedisRateLimiter( IConnectionMultiplexer redis, InMemoryRateLimiter fallback )
    {
        m_redis = redis;
        m_fallback = fallback;
    }

    public async Task<bool> TryAcquireAsync( RateLimitPolicy policy, string subject )
    {
        try
        {
            RedisResult hits = await m_redis.GetDatabase()
                .ScriptEvaluateAsync(
                    HitScript,
                    new RedisKey[] { Key( policy, subject ) },
                    new RedisValue[] { (long)policy.Window.TotalMilliseconds } )
                .DefaultConfigureAwait();
            MarkUp();
            return (long)hits <= policy.Limit;
        }
        catch (Exception ex) when (IsUnavailable( ex ))
        {
            MarkDown( ex );
            return await m_fallback.TryAcquireAsync( policy, subject ).DefaultConfigureAwait();
        }
    }

    public async Task ResetAsync( RateLimitPolicy policy, string subject )
    {
        await m_fallback.ResetAsync( policy, subject ).DefaultConfigureAwait();
        try
        {
            await m_redis.GetDatabase().KeyDeleteAsync( Key( policy, subject ) ).DefaultConfigureAwait();
            MarkUp();
        }
        catch (Exception ex) when (IsUnavailable( ex ))
        {
            MarkDown( ex );
        }
    }

    /// <summary>The subject is hashed: emails stay out of Redis and key length is bounded.</summary>
    internal static string Key( RateLimitPolicy policy, string subject )
    {
        byte[] hash = SHA256.HashData( Encoding.UTF8.GetBytes( subject ) );
        return $"principles:rl:{policy.Name}:{Convert.ToHexString( hash )}";
    }

    // RedisTimeoutException derives from TimeoutException, not RedisException.
    private static bool IsUnavailable( Exception ex ) => ex is RedisException or TimeoutException;

    private void MarkDown( Exception ex )
    {
        if (Interlocked.Exchange( ref m_isDown, 1 ) == 0)
        {
            Log.Warning( ex, "Redis is unavailable; rate limits are counted in process" );
        }
    }

    private void MarkUp()
    {
        if (Interlocked.Exchange( ref m_isDown, 0 ) == 1)
        {
            Log.Information( "Redis is available again" );
        }
    }
}
