using BusinessLogic;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

using SET.Shared.Models;
using SET.Shared.Models.Auth;

using System;
using System.Net.Mail;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SET.Shared.Helpers;
using Microsoft.Extensions.Configuration;

namespace SET.WebAPI.Controllers;

[Route( "api/account" )]
[ApiController]
public class AccountController : BaseController
{
    private const int MIN_PASSWORD_LENGTH = 8;

    private readonly IAuthService m_authService;
    private readonly IJwtTokenService m_jwtTokenService;
    private readonly IConfiguration m_configuration;

    public AccountController( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        m_authService = serviceProvider.GetService<IAuthService>();
        m_jwtTokenService = serviceProvider.GetService<IJwtTokenService>();
        m_configuration = serviceProvider.GetService<IConfiguration>();
    }

    [AllowAnonymous]
    [HttpPost( "authentication" )]
    public Task<IActionResult> Register( [FromBody] UserRegister registerInfo )
    {
        return TryCatchAsync( async () =>
        {
            #region Check parameter
            if(registerInfo is null)
            {
                return BadRequest( "RegisterInfo is null" );
            }
            #endregion

            IActionResult? result = null;

            string firstKey = m_configuration["EncryptionSettings:FirstKey"];
            string secondKey = m_configuration["EncryptionSettings:SecondKey"];

            try
            {
                string decryptedPassword = PasswordHelper.DecryptNewPassword( registerInfo.Password, firstKey, secondKey );
                if (decryptedPassword?.Length >= MIN_PASSWORD_LENGTH)
                {
                    registerInfo.Password = decryptedPassword;
                }
                else
                {
                    result = BadRequest( "PasswordLengthIsLessThanEight" );
                }
            }
            catch
            {
                result = BadRequest( "InvalidPassword" );
            }

            if(result is null)
            {
                User user = await m_authService.RegisterAsync( registerInfo ).DefaultConfigureAwait();

                RegisterResponse response = new( Message: "You are right", Token: m_jwtTokenService.GetToken( user ) );
                result = Ok( response );
            }

            return result;
        } );
    }

    [HttpPost( "authorization" )]
    public Task<IActionResult> Login( [FromBody] UserLogin userlogin )
    {
        return TryCatchAsync( async () =>
        {
            User user = await m_authService.LoginAsync( userlogin ).DefaultConfigureAwait();

            LoginResponse result = new( Message: "You are right", Token: m_jwtTokenService.GetToken( user ), user.Id );
            return Ok( result );
        } );
    }

    [HttpDelete( "{userId}" )]
    public Task<IActionResult> Delete( long userId )
    {
        return TryCatchAsync( userId, async ( user ) =>
        {
            List<UserHabit> habits = await DbContext.
                UserHabits.
                Where( u => u.UserId == userId ).
                Include( u => u.Frequency ).
                Include( u => u.Progresses ).
                Include( u => u.AreasOfLife ).
                ToListAsync().
                DefaultConfigureAwait();

            DbContext.ProgressesOfHabits.RemoveRange( habits.SelectMany( u => u.Progresses ) );
            DbContext.UserAreasOfLifeUserHabits.RemoveRange( habits.SelectMany( u => u.AreasOfLife ) );
            DbContext.UserHabits.RemoveRange( habits );
            DbContext.Frequencies.RemoveRange( habits.Select( u => u.Frequency ) );

            await DbContext.SaveChangesAsync().DefaultConfigureAwait();
            await DbContext.UserAreasOfLife.Where( u => u.UserId == userId ).ExecuteDeleteAsync().DefaultConfigureAwait();
            await DbContext.Users.Where( u => u.Id == userId ).ExecuteDeleteAsync().DefaultConfigureAwait();

            return Ok();
        } );
    }

    //It generates random code and sends it to email specified in a "request" parameter
    [HttpGet( "code" )]
    public Task<IActionResult> GenerateCode( [FromQuery] string emailWhereSendCode )
    {
        return TryCatchAsync( async () =>
        {
            #region Check parameter
            User user = await DbContext.
                Users.
                FirstOrDefaultAsync( u => u.Email == emailWhereSendCode ).
                DefaultConfigureAwait();

            if (user is null)
            {
                return BadRequest( "UserIsNotFound" );
            }

            if (string.IsNullOrWhiteSpace( emailWhereSendCode ))
            {
                return BadRequest( "EmailWhereSendCodeIsNullOrWhiteSpace" );
            }
            #endregion

            string fromEmail = "app@principles.top";
            string fromPassword = "pN8g^x47_N";

            SmtpClient smtpClient = new( host: "smtp.hostinger.com" )
            {
                Port = 587,
                Credentials = new NetworkCredential( fromEmail, fromPassword ),
                EnableSsl = true
            };

            int code = GenerateRandomCode();
            MailMessage mailMessage = new()
            {
                From = new MailAddress( fromEmail, displayName: "Principles app" ),
                Subject = "Your 6-digit code",
                Body = $"Your code is: {code}.",
                IsBodyHtml = false,
            };
            mailMessage.To.Add( emailWhereSendCode );

            await smtpClient.SendMailAsync( mailMessage ).DefaultConfigureAwait();

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
            if(request is null)
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
                FirstOrDefaultAsync( u => u.Email == request.Email ).
                DefaultConfigureAwait();

            IActionResult? result = null;

            if (user is null)
            {
                result = BadRequest( error: "UserIsNotFound" );
            }
            else
            {
                string firstKey = m_configuration["EncryptionSettings:FirstKey"];
                string secondKey = m_configuration["EncryptionSettings:SecondKey"];

                try
                {
                    string decryptedPassword = PasswordHelper.DecryptNewPassword( request.NewPassword, firstKey, secondKey );

                    if (decryptedPassword.Length >= MIN_PASSWORD_LENGTH)
                    {
                        user.Password = PasswordHelper.CreatePasswordHash( decryptedPassword );
                    }
                    else
                    {
                        result = BadRequest( "PasswordLengthIsLessThanEight" );
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
