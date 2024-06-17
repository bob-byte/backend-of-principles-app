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

[Route("api/account")]
[ApiController]
public class AccountController : BaseController
{
    private readonly IAuthService m_authService;
    private readonly IJwtTokenService m_jwtTokenService;
    private readonly IConfiguration m_configuration;

    public AccountController(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        m_authService = serviceProvider.GetService<IAuthService>();
        m_jwtTokenService = serviceProvider.GetService<IJwtTokenService>();
        m_configuration = serviceProvider.GetService<IConfiguration>();
    }

    [AllowAnonymous]
    [HttpPost( "authentication" )]
    public Task<IActionResult> Register([FromBody] UserRegister registerInfo)
    {
        return TryCatchAsync( async () =>
        {
            User user = await m_authService.RegisterAsync( registerInfo );

            RegisterResponse result = new( Message: "You are right", Token: m_jwtTokenService.GetToken( user ) );
            string jsonResult = JsonSerializer.Serialize( result );
            return Ok( jsonResult );
        } );
    }

    [HttpPost( "authorization" )]
    public Task<IActionResult> Login([FromBody] UserLogin userlogin)
    {
        return TryCatchAsync( async () =>
        {
            User user = await m_authService.LoginAsync( userlogin ).DefaultConfigureAwait();

            LoginResponse result = new( Message: "You are right", Token: m_jwtTokenService.GetToken( user ), user.Id );
            string jsonResult = JsonSerializer.Serialize( result );
            return Ok( jsonResult );
        } );
    }

    [HttpDelete("{userId}")]
    public Task<IActionResult> Delete(long userId)
    {
        return TryCatchAsync( userId, async ( user ) =>
        {
            List<UserHabit> habits = await DbContext.
                UserHabits.
                Where(u => u.UserId == userId).
                Include( u => u.Frequency ).
                Include( u => u.Progresses).
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

    [HttpPost( "email" )]
    public async Task<ActionResult<string>> SendEmail(string userEmail)
    {
        string fromEmail = "app@principles.top";
        string fromPassword = "pN8g^x47_N";
        string code = GenerateRandomCode();

        var smtpClient = new SmtpClient( "smtp.hostinger.com" )
        {
            Port = 587,
            Credentials = new NetworkCredential( fromEmail, fromPassword ),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress( fromEmail ),
            Subject = "Your 6-digit code",
            Body = $"Your code is: {code}",
            IsBodyHtml = false,
        };
        mailMessage.To.Add( userEmail );

        await smtpClient.SendMailAsync( mailMessage );

        return Ok(code);
    }
    [HttpPost( "password" )]
    public async Task<IActionResult> ChangePassword( [FromBody] UserNewPassword request )
    {
        User user = await DbContext.Users.FirstOrDefaultAsync( u => u.Email == request.Email );
        if (user == null)
        {
            return BadRequest( "UserIsNotFound" );
        }
        string firstKey = m_configuration["EncryptionSettings:FirstKey"];
        string secondKey = m_configuration["EncryptionSettings:SecondKey"];

        string decryptedPassword = PasswordHelper.DecryptNewPassword( request.NewPassword, firstKey, secondKey);

        user.Password = PasswordHelper.CreatePasswordHash(decryptedPassword);

        await DbContext.SaveChangesAsync();

        return Ok();
    }

    private string GenerateRandomCode()
    {
        Random random = new Random();
        int code = random.Next( 100000, 999999 );
        return code.ToString();
    }
}
