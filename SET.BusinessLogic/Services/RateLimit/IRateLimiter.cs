namespace BusinessLogic;

/// <summary>Fixed window: at most <paramref name="Limit"/> hits per <paramref name="Window"/>.</summary>
public sealed record RateLimitPolicy( string Name, int Limit, TimeSpan Window );

public static class RateLimitPolicies
{
    /// <summary>Password attempts per email; cleared by a successful login or password reset.</summary>
    public static readonly RateLimitPolicy Login = new( "login", 10, TimeSpan.FromMinutes( 15 ) );

    /// <summary>
    /// Guesses per emailed code; cleared when a new code is stored. The window starts at the first
    /// guess, so it must be at least as long as the code lifetime.
    /// </summary>
    public static readonly RateLimitPolicy VerificationCodeGuess =
        new( "code-guess", 5, AccountService.VerificationCodeTtl );

    /// <summary>Verification emails per address.</summary>
    public static readonly RateLimitPolicy VerificationEmail = new( "code-email", 5, TimeSpan.FromHours( 1 ) );

    /// <summary>AI calls per user.</summary>
    public static readonly RateLimitPolicy AiBurst = new( "ai-minute", 20, TimeSpan.FromMinutes( 1 ) );

    /// <summary>AI calls per user.</summary>
    public static readonly RateLimitPolicy AiDaily = new( "ai-day", 300, TimeSpan.FromDays( 1 ) );
}

public interface IRateLimiter
{
    /// <summary>Counts one hit for <paramref name="subject"/>; false when it is over the limit.</summary>
    Task<bool> TryAcquireAsync( RateLimitPolicy policy, string subject );

    Task ResetAsync( RateLimitPolicy policy, string subject );
}
