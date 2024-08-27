using AutoMapper;

using BusinessLogic;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using SET.Shared;

using System;

namespace SET.WebAPI.Controllers;

public class BaseController : ControllerBase
{
    public BaseController( IServiceProvider serviceProvider )
    {
        ServiceProvider = serviceProvider;
        DbContext = ServiceProvider.GetService<AppDbContext>();
        Mapper = ServiceProvider.GetService<IMapper>();
        m_jwtTokenService = ServiceProvider.GetRequiredService<IJwtTokenService>();
    }

    protected IServiceProvider ServiceProvider { get; }
    protected AppDbContext DbContext { get; }
    protected IMapper Mapper { get; }
    protected IJwtTokenService m_jwtTokenService { get; }

    protected async Task<IActionResult> CheckUserIdAsync(long userId)
    {
        User user = await DbContext.Users.FirstOrDefaultAsync( u => u.Id == userId ).DefaultConfigureAwait();

        IActionResult actionResult;
        if (user == null)
        {
            actionResult = BadRequest( error: "UserIsNotFound" );
        }
        else
        {
            actionResult = null;
        }

        return actionResult;
    }

    protected async Task<IActionResult> TryCatchAsync( Func<User, Task<IActionResult>> action )
    {
        IActionResult result;

        try
        {
            string? token = await HttpContext.GetTokenAsync( tokenName: "access_token" ).DefaultConfigureAwait();

            long userId = m_jwtTokenService.GetUserIdFromJwt( token );
            if (userId <= 0)
            {
                return BadRequest( "UserIdCouldNotBeRetrievedFromTheToken." );
            }

            User? user = await DbContext.Users.FindAsync( userId ).DefaultConfigureAwait();

            if (user is null)
            {
                result = BadRequest( error: "UserIsNotFound" );
            }
            else
            {
                result = await action( user ).DefaultConfigureAwait();
            }
        }
        catch (Exception ex)
        {
            result = WriteExceptionStatus( ex );
        }

        return result;
    }

    protected async Task<IActionResult> TryCatchAsync(Func<Task<IActionResult>> action)
    {
        IActionResult result;
        try
        {
            result = await action();
        }
        catch(Exception ex)
        {
            result = WriteExceptionStatus( ex );
        }

        return result;
    }

    protected IActionResult WriteExceptionStatus( Exception ex, int errorCode = 0 )
    {
        if (ex.InnerException != null)
        {
            ex = ex.InnerException;
        }

        if (errorCode == 0)
        {
            errorCode = 500;
        }

        Log.Error( ex, ex.Message );
        return new WebExceptionResult( errorCode, ex.Message );
    }
}
