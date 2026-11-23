namespace BusinessLogic;

/// <summary>Caps how many paid AI calls one user can make.</summary>
public interface IAiUsageLimiter
{
    Task<ServiceResult> TryConsumeAsync( long userId );
}
