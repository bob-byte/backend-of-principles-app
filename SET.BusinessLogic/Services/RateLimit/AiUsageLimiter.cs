using System.Globalization;

namespace BusinessLogic;

public class AiUsageLimiter : IAiUsageLimiter
{
    // Clients show the AI error text as is.
    internal const string BurstError = "Too many AI requests. Please wait a minute and try again.";
    internal const string DailyError = "Daily AI limit reached. Please try again later.";

    private readonly IRateLimiter m_rateLimiter;

    public AiUsageLimiter( IRateLimiter rateLimiter )
    {
        m_rateLimiter = rateLimiter;
    }

    public async Task<ServiceResult> TryConsumeAsync( long userId )
    {
        string subject = userId.ToString( CultureInfo.InvariantCulture );

        // Burst first, so rejected bursts do not use up the daily allowance.
        if (!await m_rateLimiter.TryAcquireAsync( RateLimitPolicies.AiBurst, subject ).DefaultConfigureAwait())
        {
            return LimitError( BurstError );
        }

        if (!await m_rateLimiter.TryAcquireAsync( RateLimitPolicies.AiDaily, subject ).DefaultConfigureAwait())
        {
            return LimitError( DailyError );
        }

        return ServiceResult.Success;
    }

    private static ServiceError LimitError( string message ) => ServiceError.TooManyRequests( new { error = message } );
}
