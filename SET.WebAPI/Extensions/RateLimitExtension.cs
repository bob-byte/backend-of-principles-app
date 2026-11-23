using BusinessLogic;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using StackExchange.Redis;

namespace SET.WebAPI.Extensions;

public static class RateLimitExtension
{
    /// <summary>Counts in Redis when <c>REDIS_CONNECTION</c> is set, otherwise in process.</summary>
    public static IServiceCollection AddRateLimiter( this IServiceCollection services, IConfiguration configuration )
    {
        services.AddSingleton( TimeProvider.System );
        services.AddSingleton<InMemoryRateLimiter>();

        string? connectionString = configuration["REDIS_CONNECTION"];
        if (string.IsNullOrWhiteSpace( connectionString ))
        {
            Log.Information( "REDIS_CONNECTION is not set; rate limits are counted in process" );
            services.AddSingleton<IRateLimiter>( sp => sp.GetRequiredService<InMemoryRateLimiter>() );
            return services;
        }

        ConfigurationOptions options = ConfigurationOptions.Parse( connectionString );
        // Redis only backs rate limits: never fail startup or hold up a request because it is down.
        options.AbortOnConnectFail = false;
        options.ConnectTimeout = 2000;
        options.AsyncTimeout = 1000;
        options.BacklogPolicy = BacklogPolicy.FailFast;

        services.AddSingleton<IConnectionMultiplexer>( _ => ConnectionMultiplexer.Connect( options ) );
        services.AddSingleton<IRateLimiter, RedisRateLimiter>();
        return services;
    }
}
