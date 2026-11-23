namespace BusinessLogic;

public interface IRefreshTokenService
{
    /// <summary>Creates a new refresh token for the user and returns the plaintext value.</summary>
    Task<string> IssueAsync( long userId, string? deviceId = null );

    /// <summary>
    /// Validates and rotates a refresh token. Returns a new access + refresh pair, or an error.
    /// </summary>
    Task<ServiceResult<LoginResponse>> RotateAsync( string refreshTokenPlain, string? deviceId = null );

    Task RevokeAsync( string refreshTokenPlain );

    Task RevokeAllForUserAsync( long userId );
}
