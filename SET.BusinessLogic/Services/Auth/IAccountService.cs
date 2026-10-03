using SET.Shared.Models.Auth;

namespace BusinessLogic;

/// <summary>
/// Account lifecycle: sign-up / sign-in (email, Google, Apple), verification codes, password change,
/// and account deletion. Client passwords arrive encrypted and are decrypted here.
/// </summary>
public interface IAccountService
{
    Task<User?> FindUserAsync( long userId );

    Task<ServiceResult<GoogleAuthResponse>> GoogleAuthAsync( GoogleLoginRequest request );

    Task<ServiceResult> RegisterAsync( UserRegister registerInfo );

    Task<ServiceResult<LoginResponse>> LoginAsync( UserLogin userLogin );

#if DEBUG
    /// <summary>Plain-text password login for local testing.</summary>
    Task<ServiceResult<LoginResponse>> SimpleLoginAsync( UserLogin userLogin );

    /// <summary>Plain-text password sign-up for local testing (skips email verification).</summary>
    Task<ServiceResult<LoginResponse>> SimpleRegisterAsync( UserRegister registerInfo );
#endif

    /// <summary>Validates the Apple identity token and signs in, creating the user when needed.</summary>
    Task<ServiceResult<LoginResponse>> AppleAuthAsync( AppleAuthRequest authRequest );

    /// <param name="verificationCode">
    /// Optional 6-digit code from <c>GET api/account/code</c>. When provided (website delete),
    /// it must match a stored password-reset code; Flutter delete may omit it.
    /// </param>
    Task<ServiceResult> DeleteAsync( User user, int? verificationCode );

    /// <summary>Emails a forget-password code; the email must belong to an existing user. Does not return the code.</summary>
    Task<ServiceResult> SendPasswordResetCodeAsync( string emailWhereSendCode, string? language );

    /// <summary>Emails a sign-up code; the email must not be registered yet. Does not return the code.</summary>
    Task<ServiceResult> SendSignupCodeAsync( string emailWhereSendCode, string? language );

    Task<ServiceResult> ChangePasswordAsync( UserNewPassword request );
}
