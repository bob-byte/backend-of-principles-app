using BusinessLogic;
using Microsoft.Extensions.DependencyInjection;
using SET.Shared.Models.Auth;
using System.Net.Mail;
using System.Net;
using SET.Shared.Helpers;
using Microsoft.Extensions.Configuration;

namespace SET.WebAPI.Controllers;

[Route( "api/account" )]
[ApiController]
public class AccountController : BaseController
{
    private const int MIN_PASSWORD_LENGTH = 8;
    private const int MAX_PASSWORD_LENGTH = 20;

    private readonly Lazy<SmtpClient> m_smtpClient;

    private readonly IAuthService m_authService;
    private readonly IJwtTokenService m_jwtTokenService;
    private readonly IConfiguration m_configuration;

    public AccountController( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        m_authService = serviceProvider.GetRequiredService<IAuthService>();
        m_jwtTokenService = serviceProvider.GetRequiredService<IJwtTokenService>();
        m_configuration = serviceProvider.GetRequiredService<IConfiguration>();

        m_smtpClient = new Lazy<SmtpClient>( () =>
        {
            string fromPassword = m_configuration["HostEmailPassword"];
            if (string.IsNullOrWhiteSpace( fromPassword ))
            {
                fromPassword = m_configuration["HOST_EMAIL_PASSWORD"];
            }

            string fromEmail = m_configuration["HostEmail"];
            SmtpClient smtpClient = new( host: "smtp.hostinger.com" )
            {
                Port = 587,
                Credentials = new NetworkCredential( fromEmail, fromPassword ),
                EnableSsl = true
            };
            return smtpClient;
        } );
    }

    ~AccountController()
    {
        if (m_smtpClient.IsValueCreated)
        {
            m_smtpClient.Value.Dispose();
        }
    }

    [HttpPost( "googleauthorization" )]
    public Task<IActionResult> GoogleAuthorization( [FromBody] GoogleLoginRequest request )
    {
        return TryCatchAsync( async () =>
        {
            #region check parameter
            if (request is null)
            {
                return BadRequest( "RequestBodyIsNull" );
            }

            if (string.IsNullOrWhiteSpace( request.AccessToken ))
            {
                return BadRequest( "AccessTokenIsNull" );
            }

            if (string.IsNullOrWhiteSpace( request.IdToken ))
            {
                return BadRequest( "IdTokenIsNull" );
            }
            #endregion

            GoogleAuthResponse response = await m_authService.GoogleAuthAsync( request.IdToken, request.AccessToken ).DefaultConfigureAwait();

            IActionResult result = Ok( response );
            return result;
        } );
    }

    [AllowAnonymous]
    [HttpPost( "authentication" )]
    public Task<IActionResult> Register( [FromBody] UserRegister registerInfo )
    {
        return TryCatchAsync( async () =>
        {
            #region Check parameter
            if (registerInfo is null)
            {
                return BadRequest( error: "RegisterInfoIsNull" );
            }

            if (registerInfo.Email is null)
            {
                return BadRequest( "EmailOfRegisterInfoIsNull" );
            }

            if (registerInfo.Password is null)
            {
                return BadRequest( "PasswordShoudBeFilled" );
            }

            bool isAlreadyRegistered = await DbContext.
                Users.
                AnyAsync( u => u.Email.ToLower() == registerInfo.Email.ToLower() ).
                DefaultConfigureAwait();
            if (isAlreadyRegistered)
            {
                return BadRequest( "UserWithIdenticalEmailAlreadyExists" );
            }
            #endregion

            IActionResult? result = null;

            string firstKey = m_configuration["EncryptionSettings:FirstKey"] ??
                m_configuration["FIRST_KEY_OF_PASSWORD_ENCRYPTION"];
            if (string.IsNullOrWhiteSpace( firstKey ))
            {
                throw new InvalidProgramException( "First key of password encryption is not set" );
            }

            string secondKey = m_configuration["EncryptionSettings:SecondKey"] ??
                m_configuration["SECOND_KEY_OF_PASSWORD_ENCRYPTION"];
            if (string.IsNullOrWhiteSpace( secondKey ))
            {
                throw new InvalidProgramException( "Second key of password encryption is not set" );
            }

            try
            {
                string decryptedPassword = PasswordHelper.DecryptNewPassword( registerInfo.Password, firstKey, secondKey );
                if (MIN_PASSWORD_LENGTH <= decryptedPassword?.Length && decryptedPassword.Length <= MAX_PASSWORD_LENGTH)
                {
                    registerInfo.Password = decryptedPassword;
                }
                else
                {
                    result = BadRequest( "PasswordLengthIsLessThanMinOrMoreThanMaxCharacters" );
                }
            }
            catch
            {
                result = BadRequest( "InvalidPassword" );
            }

            if (result is null)
            {
                await m_authService.RegisterAsync( registerInfo ).DefaultConfigureAwait();
                result = Ok();
            }

            return result;
        } );
    }

    [HttpPost( "authorization" )]
    public Task<IActionResult> Login( [FromBody] UserLogin userlogin )
    {
        return TryCatchAsync( async () =>
        {
            #region Check parameter
            if (userlogin.Password is null)
            {
                return BadRequest( "PasswordShouldBeFilled" );
            }
            #endregion

            IActionResult? result = null;

            string firstKey = m_configuration["EncryptionSettings:FirstKey"] ??
                m_configuration["FIRST_KEY_OF_PASSWORD_ENCRYPTION"];
            if (string.IsNullOrWhiteSpace( firstKey ))
            {
                throw new InvalidProgramException( "First key of password encryption is not set" );
            }

            string secondKey = m_configuration["EncryptionSettings:SecondKey"] ??
                m_configuration["SECOND_KEY_OF_PASSWORD_ENCRYPTION"];
            if (string.IsNullOrWhiteSpace( secondKey ))
            {
                throw new InvalidProgramException( "Second key of password encryption is not set" );
            }

            try
            {
                string decryptedPassword = PasswordHelper.DecryptNewPassword( userlogin.Password, firstKey, secondKey );
                userlogin.Password = decryptedPassword;
            }
            catch
            {
                //message contains "email" to confuse an attacker
                result = BadRequest( "InvalidEmailOrPassword" );
            }

            if (result is null)
            {
                (User? user, string? errorMsg) loginResult = await m_authService.LoginAsync( userlogin ).DefaultConfigureAwait();

                if (loginResult.errorMsg is null)
                {
                    User user = loginResult.user;

                    //TODO: remove returning user.Id
                    LoginResponse response = new( Message: "You are right", Token: m_jwtTokenService.GetToken( user ), user.Id );
                    result = Ok( response );
                }
                else
                {
                    result = BadRequest( loginResult.errorMsg );
                }
            }

            return result;
        } );
    }

#if DEBUG
    [HttpPost( "simpleauthorization" )]
    public Task<IActionResult> SimpleLogin( [FromBody] UserLogin userlogin )
    {
        return TryCatchAsync( async () =>
        {
            IActionResult? result = null;

            (User? user, string? errorMsg) loginResult = await m_authService.LoginAsync( userlogin ).DefaultConfigureAwait();

            if (loginResult.errorMsg is null)
            {
                User user = loginResult.user;

                LoginResponse response = new( Message: "You are right", Token: m_jwtTokenService.GetToken( user ), user.Id );
                result = Ok( response );
            }
            else
            {
                result = BadRequest( loginResult.errorMsg );
            }

            return result;
        } );
    }

    [AllowAnonymous]
    [HttpPost( "simpleauthentication" )]
    public Task<IActionResult> SimpleRegister( [FromBody] UserRegister registerInfo )
    {
        return TryCatchAsync( async () =>
        {
            #region Check parameter
            if (registerInfo is null)
            {
                return BadRequest( error: "RegisterInfoIsNull" );
            }

            if (registerInfo.Email is null)
            {
                return BadRequest( "EmailOfRegisterInfoIsNull" );
            }

            bool isAlreadyRegistered = await DbContext.Users
                .AnyAsync( u => u.Email.ToLower() == registerInfo.Email.ToLower() )
                .ConfigureAwait( false );
            if (isAlreadyRegistered)
            {
                return BadRequest( "UserWithIdenticalEmailAlreadyExists" );
            }
            #endregion

            User user = await m_authService.RegisterAsync( registerInfo ).ConfigureAwait( false );
            var response = new { Token = m_jwtTokenService.GetToken( user ) };

            IActionResult result = Ok( response );
            return result;
        } );
    }
#endif

    [Authorize( AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme )]
    [HttpDelete]
    public Task<IActionResult> Delete()
    {
        return TryCatchAsync( async ( user ) =>
        {
            List<UserHabit> habits = await DbContext.
                UserHabits.
                Where( u => u.UserId == user.Id ).
                Include( u => u.Frequency ).
                Include( u => u.Progresses ).
                Include( u => u.AreasOfLife ).
                AsSplitQuery().
                ToListAsync().
                DefaultConfigureAwait();

            DbContext.ProgressesOfHabits.RemoveRange( habits.SelectMany( u => u.Progresses ) );
            DbContext.UserAreasOfLifeUserHabits.RemoveRange( habits.SelectMany( u => u.AreasOfLife ) );
            DbContext.UserHabits.RemoveRange( habits );
            DbContext.Frequencies.RemoveRange( habits.Select( u => u.Frequency ) );

            await DbContext.SaveChangesAsync().DefaultConfigureAwait();
            await DbContext.UserAreasOfLife.Where( u => u.UserId == user.Id ).ExecuteDeleteAsync().DefaultConfigureAwait();
            await DbContext.ClientLogs.Where( u => u.UserId == user.Id ).ExecuteUpdateAsync( setPropDelegate => setPropDelegate.SetProperty( c => c.UserId, c => null ) ).DefaultConfigureAwait();
            await DbContext.Users.Where( u => u.Id == user.Id ).ExecuteDeleteAsync().DefaultConfigureAwait();

            return Ok();
        } );
    }

    //It generates random code and sends it to email specified in the "request" parameter
    [HttpGet( "code" )]
    public Task<IActionResult> GenerateCode( [FromQuery] string emailWhereSendCode )
    {
        return TryCatchAsync( async () =>
        {
            #region Check parameter
            User user = await DbContext.
                Users.
                FirstOrDefaultAsync( u => u.Email.ToLower() == emailWhereSendCode.ToLower() ).
                DefaultConfigureAwait();

            if (user is null)
            {
                return BadRequest( "EmailIsIncorrect" );
            }

            if (string.IsNullOrWhiteSpace( emailWhereSendCode ))
            {
                return BadRequest( "EmailWhereSendCodeIsNullOrWhiteSpace" );
            }
            #endregion

            int code = GenerateRandomCode();
            MailMessage mailMessage = new()
            {
                From = new MailAddress( m_configuration["HostEmail"], displayName: "Principles app" ),
                Subject = "Your 6-digit code",
                Body = $"Your code is: {code}",
                IsBodyHtml = false,
            };
            mailMessage.To.Add( emailWhereSendCode );

            await m_smtpClient.Value.SendMailAsync( mailMessage ).DefaultConfigureAwait();

            GenerateCodeResponse response = new( code );
            return Ok( response );
        } );
    }

    [HttpPut( "password" )]
    public Task<IActionResult> ChangePassword( [FromBody] UserNewPassword request )
    {
        return TryCatchAsync( async () =>
        {
            #region Check parameter
            if (request is null)
            {
                return BadRequest( error: "Request is null" );
            }

            if (string.IsNullOrWhiteSpace( request.Email ))
            {
                return BadRequest( "EmailIsNullOrWhiteSpace" );
            }

            if (string.IsNullOrWhiteSpace( request.NewPassword ))
            {
                return BadRequest( "PasswordIsNullOrWhiteSpace" );
            }
            #endregion
            User user = await DbContext.
                Users.
                FirstOrDefaultAsync( u => u.Email.ToLower() == request.Email.ToLower() ).
                DefaultConfigureAwait();

            IActionResult? result = null;

            if (user is null)
            {
                result = BadRequest( error: "EmailIsIncorrect" );
            }
            else
            {
                string firstKey = m_configuration["EncryptionSettings:FirstKey"] ??
                    m_configuration["FIRST_KEY_OF_PASSWORD_ENCRYPTION"];
                if (string.IsNullOrWhiteSpace( firstKey ))
                {
                    throw new InvalidProgramException( "First key of password encryption is not set" );
                }

                string secondKey = m_configuration["EncryptionSettings:SecondKey"] ??
                    m_configuration["SECOND_KEY_OF_PASSWORD_ENCRYPTION"];
                if (string.IsNullOrWhiteSpace( secondKey ))
                {
                    throw new InvalidProgramException( "Second key of password encryption is not set" );
                }

                try
                {
                    string decryptedPassword = PasswordHelper.DecryptNewPassword( request.NewPassword, firstKey, secondKey );

                    if (MIN_PASSWORD_LENGTH <= decryptedPassword?.Length && decryptedPassword.Length <= MAX_PASSWORD_LENGTH)
                    {
                        user.Password = PasswordHelper.CreatePasswordHash( decryptedPassword );
                    }
                    else
                    {
                        result = BadRequest( "PasswordLengthIsLessThanMinOrMoreThanMaxCharacters" );
                    }
                }
                catch
                {
                    result = BadRequest( "IncorrectPassword" );
                }

                if (result is null)
                {
                    await DbContext.SaveChangesAsync().DefaultConfigureAwait();
                    result = Ok();
                }
            }

            return result;
        } );
    }

    private static int GenerateRandomCode()
    {
        Random random = new();
        int result = random.Next( minValue: 100000, maxValue: 999999 );
        return result;
    }
}
