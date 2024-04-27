using BusinessLogic;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    public Task<IActionResult> Delete(Guid userId)
    {
        return TryCatchAsync( userId, async ( user ) =>
        {
            List<UserHabit> habits = await DbContext.
                UserHabits.
                Where(u => u.UserId == userId).
                Include( u => u.Frequency ).
                Include( u => u.Progresses).
                Include( u => u.AreasOfLife ).
                ToListAsync();

            DbContext.ProgressesOfHabits.RemoveRange( habits.SelectMany( u => u.Progresses ) );
            DbContext.UserAreasOfLifeUserHabits.RemoveRange( habits.SelectMany( u => u.AreasOfLife ) );
            DbContext.UserHabits.RemoveRange( habits );
            DbContext.Frequencies.RemoveRange( habits.Select( u => u.Frequency ) );

            await DbContext.SaveChangesAsync();
            await DbContext.UserAreasOfLife.Where( u => u.UserId == userId ).ExecuteDeleteAsync();
            await DbContext.Users.Where( u => u.Id == userId ).ExecuteDeleteAsync();

            return Ok();
        } );
    }
}
