using BusinessLogic;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using SET.Shared.Models;
using SET.Shared.Models.Auth;

using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace SET.WebAPI.Controllers;

[Route("api/account")]
[ApiController]
public class AccountController : BaseController
{
    private readonly IAuthService m_authService;
    private readonly IJwtTokenService m_jwtTokenService;

    public AccountController(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        m_authService = serviceProvider.GetService<IAuthService>();
        m_jwtTokenService = serviceProvider.GetService<IJwtTokenService>();
    }

    [AllowAnonymous]
    [HttpPost( "authentication" )]
    public Task<IActionResult> Register([FromBody] UserRegister registerInfo)
    {
        return TryCatchAsync( async () =>
        {
            bool isEmailAlreadyRegistered = await DbContext.Users.AnyAsync( x => x.Email == registerInfo.Email ).DefaultConfigureAwait();
            bool isNewEmail = !isEmailAlreadyRegistered;

            IActionResult result;

            if (isNewEmail)
            {
                User user = await m_authService.RegisterAsync( registerInfo ).DefaultConfigureAwait();

                RegisterResponse response = new( Message: "You are right", Token: m_jwtTokenService.GetToken( user ) );
                string jsonResult = JsonSerializer.Serialize( response );
                result = Ok( jsonResult );
            }
            else
            {
                result = BadRequest( "UserWithIdenticalEmailAlreadyExists" );
            }

            return result;
        } );
    }

    [HttpPost( "authorization" )]
    public Task<IActionResult> Login([FromBody] UserLogin userlogin)
    {
        return TryCatchAsync( async () =>
        {
            (User? user, string? errorMsg) loginResult = await m_authService.LoginAsync( userlogin ).DefaultConfigureAwait();

            IActionResult actionResult;
            if (loginResult.errorMsg is null)
            {
                User user = loginResult.user;

                LoginResponse result = new( Message: "You are right", Token: m_jwtTokenService.GetToken( user ), user.Id );
                string jsonResult = JsonSerializer.Serialize( result );
                actionResult = Ok( jsonResult );
            }
            else
            {
                actionResult = BadRequest( loginResult.errorMsg );
            }

            return actionResult;
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
                AsSplitQuery().
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
}
