using BusinessLogic;
using Moq;
using StackExchange.Redis;

namespace SET.UnitTests.Services;

public class RateLimiterTests
{
    private static readonly RateLimitPolicy Policy = new( "test", 2, TimeSpan.FromMinutes( 1 ) );

    private sealed class ManualTime : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = DateTimeOffset.UnixEpoch;

        public override DateTimeOffset GetUtcNow() => Now;
    }

    [Fact]
    public async Task TryAcquireAsync_OverLimit_RejectsUntilWindowEnds()
    {
        ManualTime time = new();
        InMemoryRateLimiter limiter = new( time );

        Assert.True( await limiter.TryAcquireAsync( Policy, "a" ) );
        Assert.True( await limiter.TryAcquireAsync( Policy, "a" ) );
        time.Now += Policy.Window - TimeSpan.FromSeconds( 1 );
        Assert.False( await limiter.TryAcquireAsync( Policy, "a" ) );

        time.Now += TimeSpan.FromSeconds( 1 );
        Assert.True( await limiter.TryAcquireAsync( Policy, "a" ) );
    }

    [Fact]
    public async Task TryAcquireAsync_OtherSubjectOrPolicy_CountsSeparately()
    {
        InMemoryRateLimiter limiter = new( new ManualTime() );
        for (int i = 0; i < 3; i++)
        {
            await limiter.TryAcquireAsync( Policy, "a" );
        }

        Assert.True( await limiter.TryAcquireAsync( Policy, "b" ) );
        Assert.True( await limiter.TryAcquireAsync( Policy with { Name = "other" }, "a" ) );
    }

    [Fact]
    public async Task TryAcquireAsync_PurgeDue_KeepsActiveCounters()
    {
        InMemoryRateLimiter limiter = new( new ManualTime() );
        await limiter.TryAcquireAsync( Policy, "a" );
        await limiter.TryAcquireAsync( Policy, "a" );
        for (int i = 0; i < 2000; i++)
        {
            await limiter.TryAcquireAsync( Policy, $"other{i}" );
        }

        Assert.False( await limiter.TryAcquireAsync( Policy, "a" ) );
    }

    [Fact]
    public async Task ResetAsync_OverLimit_AllowsAgain()
    {
        InMemoryRateLimiter limiter = new( new ManualTime() );
        for (int i = 0; i < 3; i++)
        {
            await limiter.TryAcquireAsync( Policy, "a" );
        }

        await limiter.ResetAsync( Policy, "a" );

        Assert.True( await limiter.TryAcquireAsync( Policy, "a" ) );
    }

    public static TheoryData<Exception> RedisFailures => new()
    {
        new RedisConnectionException( ConnectionFailureType.UnableToConnect, CommandFlags.None, "down" ),
        new RedisTimeoutException( CommandFlags.None, "slow", CommandStatus.Unknown ),
    };

    [Theory]
    [MemberData( nameof( RedisFailures ) )]
    public async Task TryAcquireAsync_RedisUnavailable_StillEnforcesLimitInProcess( Exception failure )
    {
        Mock<IConnectionMultiplexer> redis = new();
        redis.Setup( r => r.GetDatabase( It.IsAny<int>(), It.IsAny<object?>() ) ).Throws( failure );
        RedisRateLimiter limiter = new( redis.Object, new InMemoryRateLimiter( new ManualTime() ) );

        Assert.True( await limiter.TryAcquireAsync( Policy, "a" ) );
        Assert.True( await limiter.TryAcquireAsync( Policy, "a" ) );
        Assert.False( await limiter.TryAcquireAsync( Policy, "a" ) );

        await limiter.ResetAsync( Policy, "a" );
        Assert.True( await limiter.TryAcquireAsync( Policy, "a" ) );
    }

    [Fact]
    public void Key_Email_IsHashedAndPrefixed()
    {
        string key = RedisRateLimiter.Key( Policy, "me@example.com" );

        Assert.StartsWith( "principles:rl:test:", key );
        Assert.DoesNotContain( "example", key );
    }
}
