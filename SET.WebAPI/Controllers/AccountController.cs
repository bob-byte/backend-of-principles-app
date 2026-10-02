using Microsoft.Extensions.DependencyInjection;

using SET.Shared.Models.Auth;

namespace SET.WebAPI.Controllers;

[Route( "api/account" )]
[ApiController]
public class AccountController : BaseController
{
    private readonly IAccountService m_accountService;

    public AccountController( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        m_accountService = serviceProvider.GetRequiredService<IAccountService>();
    }

    [HttpPost( "googleauthorization" )]
    public Task<IActionResult> GoogleAuthorization( [FromBody] GoogleLoginRequest request )
    {
        return TryCatchAsync( async () =>
            ToActionResult( await m_accountService.GoogleAuthAsync( request ).DefaultConfigureAwait() ) );
    }

    [HttpPost( "authentication" )]
    public Task<IActionResult> Register( [FromBody] UserRegister registerInfo )
    {
        return TryCatchAsync( async () =>
            ToActionResult( await m_accountService.RegisterAsync( registerInfo ).DefaultConfigureAwait() ) );
    }

    [HttpPost( "authorization" )]
    public Task<IActionResult> Login( [FromBody] UserLogin userlogin )
    {
        return TryCatchAsync( async () =>
            ToActionResult( await m_accountService.LoginAsync( userlogin ).DefaultConfigureAwait() ) );
    }

#if DEBUG
    [HttpPost( "simpleauthorization" )]
    public Task<IActionResult> SimpleLogin( [FromBody] UserLogin userlogin )
    {
        return TryCatchAsync( async () =>
            ToActionResult( await m_accountService.SimpleLoginAsync( userlogin ).DefaultConfigureAwait() ) );
    }

    [HttpPost( "simpleauthentication" )]
    public Task<IActionResult> SimpleRegister( [FromBody] UserRegister registerInfo )
    {
        return TryCatchAsync( async () =>
            ToActionResult( await m_accountService.SimpleRegisterAsync( registerInfo ).DefaultConfigureAwait() ) );
    }
#endif

    [HttpPost( "appleauthorization" )]
    public Task<IActionResult> AppleAuth( [FromBody] AppleAuthRequest authRequest )
    {
        return TryCatchAsync( async () =>
            ToActionResult( await m_accountService.AppleAuthAsync( authRequest ).DefaultConfigureAwait() ) );
    }

    [Authorize( AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme )]
    [HttpDelete]
    public Task<IActionResult> Delete()
    {
        return TryCatchAsync( async ( user ) =>
        {
            await m_accountService.DeleteAsync( user ).DefaultConfigureAwait();
            return Ok();
        } );
    }

    //It generates random code and sends it to email specified in the "request" parameter
    [HttpGet( "code" )]
    public Task<IActionResult> GenerateCode(
        [FromQuery] string emailWhereSendCode,
        [FromQuery] string? language = null )
    {
        return TryCatchAsync( async () =>
            ToActionResult( await m_accountService
                .SendPasswordResetCodeAsync( emailWhereSendCode, language )
                .DefaultConfigureAwait() ) );
    }

    /// <summary>
    /// Sends a signup verification code to an email that is not yet registered.
    /// Used so new users prove they own the address before <c>POST authentication</c>.
    /// </summary>
    [HttpGet( "signupcode" )]
    public Task<IActionResult> GenerateSignupCode(
        [FromQuery] string emailWhereSendCode,
        [FromQuery] string? language = null )
    {
        return TryCatchAsync( async () =>
            ToActionResult( await m_accountService
                .SendSignupCodeAsync( emailWhereSendCode, language )
                .DefaultConfigureAwait() ) );
    }

    [HttpGet( "apikey" )]
    [Authorize( AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme )]
    public IActionResult GetOpenAiKey()
    {
        return TryCatch( () => Ok( m_accountService.GetEncryptedAiApiKey() ) );
    }

    [HttpPut( "password" )]
    public Task<IActionResult> ChangePassword( [FromBody] UserNewPassword request )
    {
        return TryCatchAsync( async () =>
            ToActionResult( await m_accountService.ChangePasswordAsync( request ).DefaultConfigureAwait() ) );
    }
}
