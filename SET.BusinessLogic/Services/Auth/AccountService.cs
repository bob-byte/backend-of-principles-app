using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

using SET.Shared.Helpers;
using SET.Shared.Models.Auth;

using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BusinessLogic;

public class AccountService : IAccountService
{
    private const int MIN_PASSWORD_LENGTH = 8;
    private const int MAX_PASSWORD_LENGTH = 20;
    internal static readonly TimeSpan VerificationCodeTtl = TimeSpan.FromMinutes( 15 );

    private readonly AppDbContext m_dbContext;
    private readonly IAuthService m_authService;
    private readonly IJwtTokenService m_jwtTokenService;
    private readonly IRefreshTokenService m_refreshTokenService;
    private readonly IEmailSender m_emailSender;
    private readonly IConfiguration m_configuration;
    private readonly IRateLimiter m_rateLimiter;

    public AccountService(
        AppDbContext dbContext,
        IAuthService authService,
        IJwtTokenService jwtTokenService,
        IRefreshTokenService refreshTokenService,
        IEmailSender emailSender,
        IConfiguration configuration,
        IRateLimiter rateLimiter )
    {
        m_dbContext = dbContext;
        m_authService = authService;
        m_jwtTokenService = jwtTokenService;
        m_refreshTokenService = refreshTokenService;
        m_emailSender = emailSender;
        m_configuration = configuration;
        m_rateLimiter = rateLimiter;
    }

    public async Task<User?> FindUserAsync( long userId )
    {
        return await m_dbContext.Users.FindAsync( userId ).DefaultConfigureAwait();
    }

    public async Task<ServiceResult<GoogleAuthResponse>> GoogleAuthAsync(
        GoogleLoginRequest request,
        string? deviceId = null )
    {
        #region check parameter
        if (request is null)
        {
            return ServiceError.BadRequest( "RequestBodyIsNull" );
        }

        if (string.IsNullOrWhiteSpace( request.AccessToken ))
        {
            return ServiceError.BadRequest( "AccessTokenIsNull" );
        }

        if (string.IsNullOrWhiteSpace( request.IdToken ))
        {
            return ServiceError.BadRequest( "IdTokenIsNull" );
        }
        #endregion

        GoogleAuthResponse auth = await m_authService
            .GoogleAuthAsync( request.IdToken, request.AccessToken )
            .DefaultConfigureAwait();
        long userId = m_jwtTokenService.GetUserIdFromJwt( auth.Token );
        string refresh = await m_refreshTokenService.IssueAsync( userId, deviceId ).DefaultConfigureAwait();
        return new GoogleAuthResponse( auth.Token, refresh );
    }

    public async Task<ServiceResult> RegisterAsync( UserRegister registerInfo )
    {
        #region Check parameter
        if (registerInfo is null)
        {
            return ServiceError.BadRequest( "RegisterInfoIsNull" );
        }

        if (registerInfo.Email is null)
        {
            return ServiceError.BadRequest( "EmailOfRegisterInfoIsNull" );
        }

        if (registerInfo.Password is null)
        {
            return ServiceError.BadRequest( "PasswordShouldBeFilled" );
        }

        if (registerInfo.Code is null)
        {
            return ServiceError.BadRequest( "VerificationCodeIsRequired" );
        }

        registerInfo.Name ??= string.Empty;

        if (await IsEmailRegisteredAsync( registerInfo.Email ).DefaultConfigureAwait())
        {
            return ServiceError.BadRequest( "UserWithIdenticalEmailAlreadyExists" );
        }
        #endregion

        (string firstKey, string secondKey) = GetPasswordEncryptionKeys();

        try
        {
            string decryptedPassword = PasswordHelper.DecryptNewPassword( registerInfo.Password, firstKey, secondKey );
            if (!IsAllowedPasswordLength( decryptedPassword ))
            {
                return ServiceError.BadRequest( "PasswordLengthIsLessThanMinOrMoreThanMaxCharacters" );
            }

            registerInfo.Password = decryptedPassword;
        }
        catch
        {
            return ServiceError.BadRequest( "InvalidPassword" );
        }

        ServiceResult codeResult = await ConsumeVerificationCodeAsync(
            registerInfo.Email,
            EmailVerificationPurposes.Signup,
            registerInfo.Code.Value ).DefaultConfigureAwait();
        if (!codeResult.IsSuccess)
        {
            return codeResult;
        }

        await m_authService.RegisterAsync( registerInfo ).DefaultConfigureAwait();
        return ServiceResult.Success;
    }

    public async Task<ServiceResult<LoginResponse>> LoginAsync(
        UserLogin userLogin,
        string? deviceId = null )
    {
        #region Check parameter
        if (userLogin is null)
        {
            return ServiceError.BadRequest( "UserLoginRequestObjectIsNull" );
        }
        if (userLogin.Email is null)
        {
            return ServiceError.BadRequest( "EmailShouldBeFilled" );
        }
        if (userLogin.Password is null)
        {
            return ServiceError.BadRequest( "PasswordShouldBeFilled" );
        }
        #endregion

        (string firstKey, string secondKey) = GetPasswordEncryptionKeys();

        // Counted before the check, so parallel attempts cannot slip past the limit.
        string loginSubject = NormalizeEmail( userLogin.Email );
        if (!await m_rateLimiter.TryAcquireAsync( RateLimitPolicies.Login, loginSubject ).DefaultConfigureAwait())
        {
            return ServiceError.TooManyRequests( "TooManyLoginAttempts" );
        }

        try
        {
            userLogin.Password = PasswordHelper.DecryptNewPassword( userLogin.Password, firstKey, secondKey );
        }
        catch
        {
            //message contains "email" to confuse an attacker
            return ServiceError.BadRequest( "InvalidEmailOrPassword" );
        }

        (User? user, string? errorMsg) = await m_authService.LoginAsync( userLogin ).DefaultConfigureAwait();
        if (errorMsg is null)
        {
            await m_rateLimiter.ResetAsync( RateLimitPolicies.Login, loginSubject ).DefaultConfigureAwait();
            return await CreateSessionAsync( user!, deviceId ).DefaultConfigureAwait();
        }

        if (errorMsg == "EmailIsIncorrect" || errorMsg == "PasswordIsIncorrect")
        {
            return ServiceError.BadRequest( "InvalidEmailOrPassword" );
        }

        return ServiceError.BadRequest( errorMsg );
    }

#if DEBUG
    public async Task<ServiceResult<LoginResponse>> SimpleLoginAsync(
        UserLogin userLogin,
        string? deviceId = null )
    {
        (User? user, string? errorMsg) = await m_authService.LoginAsync( userLogin ).DefaultConfigureAwait();
        if (errorMsg is null)
        {
            return await CreateSessionAsync( user!, deviceId ).DefaultConfigureAwait();
        }

        return ServiceError.BadRequest( errorMsg );
    }

    public async Task<ServiceResult<LoginResponse>> SimpleRegisterAsync(
        UserRegister registerInfo,
        string? deviceId = null )
    {
        #region Check parameter
        if (registerInfo is null)
        {
            return ServiceError.BadRequest( "RegisterInfoIsNull" );
        }

        if (registerInfo.Email is null)
        {
            return ServiceError.BadRequest( "EmailOfRegisterInfoIsNull" );
        }

        if (await IsEmailRegisteredAsync( registerInfo.Email ).DefaultConfigureAwait())
        {
            return ServiceError.BadRequest( "UserWithIdenticalEmailAlreadyExists" );
        }
        #endregion

        User user = await m_authService.RegisterAsync( registerInfo ).DefaultConfigureAwait();
        return await CreateSessionAsync( user, deviceId ).DefaultConfigureAwait();
    }
#endif

    public async Task<ServiceResult<LoginResponse>> RefreshTokenAsync(
        RefreshTokenRequest request,
        string? deviceId = null )
    {
        if (request is null)
        {
            return ServiceError.BadRequest( "RequestBodyIsNull" );
        }

        return await m_refreshTokenService
            .RotateAsync( request.RefreshToken ?? string.Empty, deviceId )
            .DefaultConfigureAwait();
    }

    public async Task<ServiceResult> LogoutAsync( RefreshTokenRequest? request )
    {
        if (!string.IsNullOrWhiteSpace( request?.RefreshToken ))
        {
            await m_refreshTokenService.RevokeAsync( request!.RefreshToken! ).DefaultConfigureAwait();
        }

        return ServiceResult.Success;
    }

    public async Task<ServiceResult<LoginResponse>> AppleAuthAsync(
        AppleAuthRequest authRequest,
        string? deviceId = null )
    {
        if (authRequest == null || string.IsNullOrEmpty( authRequest.IdToken ))
        {
            return ServiceError.BadRequest( "Identity token is required to auth using apple." );
        }

        var tokenHandler = new JwtSecurityTokenHandler();

        using var client = new HttpClient();
        string keys = await client.GetStringAsync( "https://appleid.apple.com/auth/keys" ).DefaultConfigureAwait();
        IList<SecurityKey>? signingKeys = new JsonWebKeySet( keys ).GetSigningKeys();

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "https://appleid.apple.com",
            ValidateAudience = true,
            ValidAudience = "com.set.principles",
            ValidateLifetime = true,
            RequireSignedTokens = true,
            IssuerSigningKeyResolver = ( _, _, _, _ ) => signingKeys
        };

        ClaimsPrincipal principal =
            tokenHandler.ValidateToken( authRequest.IdToken, validationParameters, out _ );
        string fullName = principal.FindFirst( ClaimTypes.Name )?.Value ?? string.Empty;
        string? email = principal.FindFirst( ClaimTypes.Email )?.Value;

        if (string.IsNullOrWhiteSpace( email ))
        {
            return ServiceError.BadRequest( "Invalid token, because no email address was found." );
        }

        User? user = await m_dbContext.Users.FirstOrDefaultAsync( u => u.Email.ToLower() == email.ToLower() )
            .DefaultConfigureAwait();

        if (user is null)
        {
            UserRegister userRegister = new() { Email = email, Name = fullName, };
            user = await m_authService.RegisterAsync( userRegister ).DefaultConfigureAwait();
        }

        return await CreateSessionAsync( user, deviceId ).DefaultConfigureAwait();
    }

    public async Task<ServiceResult> DeleteAsync( User user, int? verificationCode )
    {
        if (verificationCode is not null)
        {
            ServiceResult codeResult = await ConsumeVerificationCodeAsync(
                user.Email,
                EmailVerificationPurposes.PasswordReset,
                verificationCode.Value ).DefaultConfigureAwait();
            if (!codeResult.IsSuccess)
            {
                return codeResult;
            }
        }

        await using IDbContextTransaction tran = await m_dbContext.Database.BeginTransactionAsync().DefaultConfigureAwait();

        try
        {
            List<UserHabit> habits = await m_dbContext.UserHabits.Where( u => u.UserId == user.Id )
                .Include( u => u.Frequency ).Include( u => u.Progresses )
                .AsSplitQuery().ToListAsync().DefaultConfigureAwait();

            m_dbContext.ProgressesOfHabits.RemoveRange( habits.SelectMany( u => u.Progresses ) );
            m_dbContext.UserHabits.RemoveRange( habits );
            m_dbContext.Frequencies.RemoveRange( habits.Select( u => u.Frequency ) );

            await m_dbContext.SaveChangesAsync().DefaultConfigureAwait();
            await m_dbContext.UserAreasOfLife.Where( u => u.UserId == user.Id ).ExecuteDeleteAsync()
                .DefaultConfigureAwait();
            await m_dbContext.ClientLogs.Where( u => u.UserId == user.Id )
                .ExecuteUpdateAsync( setPropDelegate => setPropDelegate.SetProperty( c => c.UserId, c => null ) )
                .DefaultConfigureAwait();
            await m_dbContext.EmailVerificationCodes
                .Where( c => c.Email == NormalizeEmail( user.Email ) )
                .ExecuteDeleteAsync()
                .DefaultConfigureAwait();
            await m_dbContext.Users.Where( u => u.Id == user.Id ).ExecuteDeleteAsync().DefaultConfigureAwait();

            await tran.CommitAsync().DefaultConfigureAwait();
        }
        catch
        {
            await tran.RollbackAsync().DefaultConfigureAwait();
            throw;
        }

        return ServiceResult.Success;
    }

    public async Task<ServiceResult> SendPasswordResetCodeAsync( string emailWhereSendCode, string? language )
    {
        #region Check parameter
        if (string.IsNullOrWhiteSpace( emailWhereSendCode ))
        {
            return ServiceError.BadRequest( "EmailWhereSendCodeIsNullOrWhiteSpace" );
        }

        User? user = await m_dbContext.
            Users.
            FirstOrDefaultAsync( u => u.Email.ToLower() == emailWhereSendCode.ToLower() ).
            DefaultConfigureAwait();

        if (user is null)
        {
            return ServiceError.BadRequest( "EmailIsIncorrect" );
        }
        #endregion

        if (!await CanSendVerificationEmailAsync( emailWhereSendCode ).DefaultConfigureAwait())
        {
            return ServiceError.TooManyRequests( "TooManyVerificationEmails" );
        }

        int code = GenerateRandomCode();
        await StoreVerificationCodeAsync( emailWhereSendCode, EmailVerificationPurposes.PasswordReset, code )
            .DefaultConfigureAwait();
        await SendVerificationEmailAsync(
            emailWhereSendCode,
            VerificationEmailContent.Purpose.PasswordReset,
            code,
            language ).DefaultConfigureAwait();

        return ServiceResult.Success;
    }

    public async Task<ServiceResult> SendSignupCodeAsync( string emailWhereSendCode, string? language )
    {
        #region Check parameter
        if (string.IsNullOrWhiteSpace( emailWhereSendCode ))
        {
            return ServiceError.BadRequest( "EmailWhereSendCodeIsNullOrWhiteSpace" );
        }

        if (await IsEmailRegisteredAsync( emailWhereSendCode ).DefaultConfigureAwait())
        {
            return ServiceError.BadRequest( "UserWithIdenticalEmailAlreadyExists" );
        }
        #endregion

        if (!await CanSendVerificationEmailAsync( emailWhereSendCode ).DefaultConfigureAwait())
        {
            return ServiceError.TooManyRequests( "TooManyVerificationEmails" );
        }

        int code = GenerateRandomCode();
        await StoreVerificationCodeAsync( emailWhereSendCode, EmailVerificationPurposes.Signup, code )
            .DefaultConfigureAwait();
        await SendVerificationEmailAsync(
            emailWhereSendCode,
            VerificationEmailContent.Purpose.Signup,
            code,
            language ).DefaultConfigureAwait();

        return ServiceResult.Success;
    }

    public async Task<ServiceResult> ChangePasswordAsync( UserNewPassword request )
    {
        #region Check parameter
        if (request is null)
        {
            return ServiceError.BadRequest( "Request is null" );
        }

        if (string.IsNullOrWhiteSpace( request.Email ))
        {
            return ServiceError.BadRequest( "EmailIsNullOrWhiteSpace" );
        }

        if (string.IsNullOrWhiteSpace( request.NewPassword ))
        {
            return ServiceError.BadRequest( "PasswordIsNullOrWhiteSpace" );
        }

        if (request.Code is null)
        {
            return ServiceError.BadRequest( "VerificationCodeIsRequired" );
        }
        #endregion

        User? user = await m_dbContext.
            Users.
            FirstOrDefaultAsync( u => u.Email.ToLower() == request.Email.ToLower() ).
            DefaultConfigureAwait();

        if (user is null)
        {
            return ServiceError.BadRequest( "EmailIsIncorrect" );
        }

        (string firstKey, string secondKey) = GetPasswordEncryptionKeys();
        string decryptedPassword;
        try
        {
            decryptedPassword = PasswordHelper.DecryptNewPassword( request.NewPassword, firstKey, secondKey );
            if (!IsAllowedPasswordLength( decryptedPassword ))
            {
                return ServiceError.BadRequest( "PasswordLengthIsLessThanMinOrMoreThanMaxCharacters" );
            }
        }
        catch
        {
            return ServiceError.BadRequest( "IncorrectPassword" );
        }

        ServiceResult codeResult = await ConsumeVerificationCodeAsync(
            request.Email,
            EmailVerificationPurposes.PasswordReset,
            request.Code.Value ).DefaultConfigureAwait();
        if (!codeResult.IsSuccess)
        {
            return codeResult;
        }

        user.Password = PasswordHelper.CreatePasswordHash( decryptedPassword );
        // Shares this DbContext, so the new password and the revocations save together.
        await m_refreshTokenService.RevokeAllForUserAsync( user.Id ).DefaultConfigureAwait();
        await m_dbContext.SaveChangesAsync().DefaultConfigureAwait();
        // Otherwise failed logins that led to the reset would keep the new password locked out.
        await m_rateLimiter
            .ResetAsync( RateLimitPolicies.Login, NormalizeEmail( request.Email ) )
            .DefaultConfigureAwait();
        return ServiceResult.Success;
    }

    private async Task<LoginResponse> CreateSessionAsync( User user, string? deviceId )
    {
        string access = m_jwtTokenService.GetToken( user );
        string refresh = await m_refreshTokenService.IssueAsync( user.Id, deviceId ).DefaultConfigureAwait();
        return new LoginResponse( access, refresh );
    }

    private Task<bool> IsEmailRegisteredAsync( string email )
    {
        return m_dbContext.Users.AnyAsync( u => u.Email.ToLower() == email.ToLower() );
    }

    private (string FirstKey, string SecondKey) GetPasswordEncryptionKeys()
    {
        string? firstKey = m_configuration["EncryptionSettings:FirstKey"] ??
            m_configuration["FIRST_KEY_OF_PASSWORD_ENCRYPTION"];
        if (string.IsNullOrWhiteSpace( firstKey ))
        {
            throw new InvalidProgramException( "First key of password encryption is not set" );
        }

        string? secondKey = m_configuration["EncryptionSettings:SecondKey"] ??
            m_configuration["SECOND_KEY_OF_PASSWORD_ENCRYPTION"];
        if (string.IsNullOrWhiteSpace( secondKey ))
        {
            throw new InvalidProgramException( "Second key of password encryption is not set" );
        }

        return (firstKey, secondKey);
    }

    private static bool IsAllowedPasswordLength( string? password )
    {
        return MIN_PASSWORD_LENGTH <= password?.Length && password.Length <= MAX_PASSWORD_LENGTH;
    }

    private Task SendVerificationEmailAsync(
        string toEmail,
        VerificationEmailContent.Purpose purpose,
        int code,
        string? language )
    {
        VerificationEmailContent.Message message =
            VerificationEmailContent.Build( purpose, code, language );
        return m_emailSender.SendAsync(
            toEmail: toEmail,
            subject: message.Subject,
            plainTextBody: message.PlainTextBody,
            htmlBody: message.HtmlBody );
    }

    private async Task StoreVerificationCodeAsync( string email, string purpose, int code )
    {
        string normalized = NormalizeEmail( email );
        string hash = HashVerificationCode( code );
        DateTime now = DateTime.UtcNow;

        EmailVerificationCode? existing = await m_dbContext.EmailVerificationCodes
            .FirstOrDefaultAsync( c => c.Email == normalized && c.Purpose == purpose )
            .DefaultConfigureAwait();

        if (existing is null)
        {
            m_dbContext.EmailVerificationCodes.Add( new EmailVerificationCode
            {
                Email = normalized,
                Purpose = purpose,
                CodeHash = hash,
                CreatedAt = now,
                ExpiresAt = now.Add( VerificationCodeTtl ),
            } );
        }
        else
        {
            existing.CodeHash = hash;
            existing.CreatedAt = now;
            existing.ExpiresAt = now.Add( VerificationCodeTtl );
        }

        await m_dbContext.SaveChangesAsync().DefaultConfigureAwait();
        await m_rateLimiter
            .ResetAsync( RateLimitPolicies.VerificationCodeGuess, GuessSubject( normalized, purpose ) )
            .DefaultConfigureAwait();
    }

    private async Task<ServiceResult> ConsumeVerificationCodeAsync( string email, string purpose, int code )
    {
        if (code is < 100000 or > 999999)
        {
            return ServiceError.BadRequest( "InvalidVerificationCode" );
        }

        string normalized = NormalizeEmail( email );
        EmailVerificationCode? stored = await m_dbContext.EmailVerificationCodes
            .FirstOrDefaultAsync( c => c.Email == normalized && c.Purpose == purpose )
            .DefaultConfigureAwait();

        if (stored is null)
        {
            return ServiceError.BadRequest( "InvalidVerificationCode" );
        }

        if (stored.ExpiresAt < DateTime.UtcNow)
        {
            m_dbContext.EmailVerificationCodes.Remove( stored );
            await m_dbContext.SaveChangesAsync().DefaultConfigureAwait();
            return ServiceError.BadRequest( "VerificationCodeExpired" );
        }

        // Counted before the comparison, so parallel guesses cannot slip past the limit. A used-up
        // code reports the ordinary token, which every released client already handles.
        bool canGuess = await m_rateLimiter
            .TryAcquireAsync( RateLimitPolicies.VerificationCodeGuess, GuessSubject( normalized, purpose ) )
            .DefaultConfigureAwait();
        if (!canGuess)
        {
            return ServiceError.BadRequest( "InvalidVerificationCode" );
        }

        if (!FixedTimeEquals( stored.CodeHash, HashVerificationCode( code ) ))
        {
            return ServiceError.BadRequest( "InvalidVerificationCode" );
        }

        m_dbContext.EmailVerificationCodes.Remove( stored );
        await m_dbContext.SaveChangesAsync().DefaultConfigureAwait();
        return ServiceResult.Success;
    }

    private Task<bool> CanSendVerificationEmailAsync( string email )
    {
        return m_rateLimiter.TryAcquireAsync( RateLimitPolicies.VerificationEmail, NormalizeEmail( email ) );
    }

    private static string GuessSubject( string normalizedEmail, string purpose ) => $"{purpose}:{normalizedEmail}";

    private static string NormalizeEmail( string email ) => email.Trim().ToLowerInvariant();

    private static string HashVerificationCode( int code )
    {
        byte[] bytes = SHA256.HashData( Encoding.UTF8.GetBytes( code.ToString( "D6" ) ) );
        return Convert.ToHexString( bytes );
    }

    private static bool FixedTimeEquals( string left, string right )
    {
        byte[] a = Encoding.UTF8.GetBytes( left );
        byte[] b = Encoding.UTF8.GetBytes( right );
        return a.Length == b.Length && CryptographicOperations.FixedTimeEquals( a, b );
    }

    private static int GenerateRandomCode() => RandomNumberGenerator.GetInt32( 100000, 1000000 );
}
