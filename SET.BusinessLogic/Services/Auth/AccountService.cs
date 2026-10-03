using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

using SET.Shared.Helpers;
using SET.Shared.Models.Auth;

using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;

namespace BusinessLogic;

public class AccountService : IAccountService
{
    private const int MIN_PASSWORD_LENGTH = 8;
    private const int MAX_PASSWORD_LENGTH = 20;

    private readonly AppDbContext m_dbContext;
    private readonly IAuthService m_authService;
    private readonly IJwtTokenService m_jwtTokenService;
    private readonly IEmailSender m_emailSender;
    private readonly IConfiguration m_configuration;

    public AccountService(
        AppDbContext dbContext,
        IAuthService authService,
        IJwtTokenService jwtTokenService,
        IEmailSender emailSender,
        IConfiguration configuration )
    {
        m_dbContext = dbContext;
        m_authService = authService;
        m_jwtTokenService = jwtTokenService;
        m_emailSender = emailSender;
        m_configuration = configuration;
    }

    public async Task<User?> FindUserAsync( long userId )
    {
        return await m_dbContext.Users.FindAsync( userId ).DefaultConfigureAwait();
    }

    public async Task<ServiceResult<GoogleAuthResponse>> GoogleAuthAsync( GoogleLoginRequest request )
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

        return await m_authService.GoogleAuthAsync( request.IdToken, request.AccessToken ).DefaultConfigureAwait();
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

        await m_authService.RegisterAsync( registerInfo ).DefaultConfigureAwait();
        return ServiceResult.Success;
    }

    public async Task<ServiceResult<LoginResponse>> LoginAsync( UserLogin userLogin )
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
            return new LoginResponse( Token: m_jwtTokenService.GetToken( user! ) );
        }

        if (errorMsg == "EmailIsIncorrect" || errorMsg == "PasswordIsIncorrect")
        {
            return ServiceError.BadRequest( "InvalidEmailOrPassword" );
        }

        return ServiceError.BadRequest( errorMsg );
    }

#if DEBUG
    public async Task<ServiceResult<LoginResponse>> SimpleLoginAsync( UserLogin userLogin )
    {
        (User? user, string? errorMsg) = await m_authService.LoginAsync( userLogin ).DefaultConfigureAwait();
        if (errorMsg is null)
        {
            return new LoginResponse( Token: m_jwtTokenService.GetToken( user! ) );
        }

        return ServiceError.BadRequest( errorMsg );
    }

    public async Task<ServiceResult<LoginResponse>> SimpleRegisterAsync( UserRegister registerInfo )
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
        return new LoginResponse( Token: m_jwtTokenService.GetToken( user ) );
    }
#endif

    public async Task<ServiceResult<LoginResponse>> AppleAuthAsync( AppleAuthRequest authRequest )
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

        return new LoginResponse( Token: m_jwtTokenService.GetToken( user ) );
    }

    public async Task DeleteAsync( User user )
    {
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
            await m_dbContext.Users.Where( u => u.Id == user.Id ).ExecuteDeleteAsync().DefaultConfigureAwait();

            await tran.CommitAsync().DefaultConfigureAwait();
        }
        catch
        {
            await tran.RollbackAsync().DefaultConfigureAwait();
            throw;
        }
    }

    public async Task<ServiceResult<GenerateCodeResponse>> SendPasswordResetCodeAsync( string emailWhereSendCode, string? language )
    {
        #region Check parameter
        User? user = await m_dbContext.
            Users.
            FirstOrDefaultAsync( u => u.Email.ToLower() == emailWhereSendCode.ToLower() ).
            DefaultConfigureAwait();

        if (user is null)
        {
            return ServiceError.BadRequest( "EmailIsIncorrect" );
        }

        if (string.IsNullOrWhiteSpace( emailWhereSendCode ))
        {
            return ServiceError.BadRequest( "EmailWhereSendCodeIsNullOrWhiteSpace" );
        }
        #endregion

        int code = GenerateRandomCode();
        await SendVerificationEmailAsync(
            emailWhereSendCode,
            VerificationEmailContent.Purpose.PasswordReset,
            code,
            language ).DefaultConfigureAwait();

        return new GenerateCodeResponse( code );
    }

    public async Task<ServiceResult<GenerateCodeResponse>> SendSignupCodeAsync( string emailWhereSendCode, string? language )
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

        int code = GenerateRandomCode();
        await SendVerificationEmailAsync(
            emailWhereSendCode,
            VerificationEmailContent.Purpose.Signup,
            code,
            language ).DefaultConfigureAwait();

        return new GenerateCodeResponse( code );
    }

    public EncryptedValueResponse GetEncryptedAiApiKey()
    {
        string? apiKey = m_configuration["AI_API_KEY"];
        if (string.IsNullOrWhiteSpace( apiKey ))
        {
            throw new InvalidOperationException( "API key not found in configuration." );
        }

        string? firstKey = m_configuration["FIRST_KEY_OF_AI_API_ENCRYPTION"] ??
                           m_configuration["EncryptionSettingsForAiApi:FirstKey"];

        string? secondKey = m_configuration["SECOND_KEY_OF_AI_API_ENCRYPTION"] ??
                            m_configuration["EncryptionSettingsForAiApi:SecondKey"];

        string encryptedApiKey = TextEncryptHelper.EncryptText( apiKey, firstKey, secondKey );

        return new EncryptedValueResponse { Value = encryptedApiKey };
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

        try
        {
            string decryptedPassword = PasswordHelper.DecryptNewPassword( request.NewPassword, firstKey, secondKey );
            if (!IsAllowedPasswordLength( decryptedPassword ))
            {
                return ServiceError.BadRequest( "PasswordLengthIsLessThanMinOrMoreThanMaxCharacters" );
            }

            user.Password = PasswordHelper.CreatePasswordHash( decryptedPassword );
        }
        catch
        {
            return ServiceError.BadRequest( "IncorrectPassword" );
        }

        await m_dbContext.SaveChangesAsync().DefaultConfigureAwait();
        return ServiceResult.Success;
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

    private static int GenerateRandomCode()
    {
        Random random = new();
        int result = random.Next( minValue: 100000, maxValue: 999999 );
        return result;
    }
}
