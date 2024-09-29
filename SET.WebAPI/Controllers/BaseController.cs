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

[Route( "api" )]
public class BaseController : ControllerBase
{
    public BaseController( IServiceProvider serviceProvider )
    {
        ServiceProvider = serviceProvider;
        DbContext = ServiceProvider.GetService<AppDbContext>();
        Mapper = ServiceProvider.GetService<IMapper>();
        JwtTokenService = ServiceProvider.GetRequiredService<IJwtTokenService>();
    }

    protected IServiceProvider ServiceProvider { get; }
    protected AppDbContext DbContext { get; }
    protected IMapper Mapper { get; }
    protected IJwtTokenService JwtTokenService { get; }

    protected async Task<IActionResult> TryCatchAsync( Func<User, Task<IActionResult>> action, object? request = null )
    {
        IActionResult result;

        try
        {
            string? token = await HttpContext.GetTokenAsync( tokenName: "access_token" ).DefaultConfigureAwait();

            long userId = JwtTokenService.GetUserIdFromJwt( token );
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
            result = WriteExceptionStatus( ex, request );
        }

        return result;
    }

    protected async Task<IActionResult> TryCatchAsync( Func<Task<IActionResult>> action, object? request = null )
    {
        IActionResult result;
        try
        {
            result = await action();
        }
        catch (Exception ex)
        {
            result = WriteExceptionStatus( ex, request );
        }

        return result;
    }

    protected IActionResult WriteExceptionStatus( Exception ex, object? request = null, int errorCode = 0 )
    {
        if (ex.InnerException != null)
        {
            ex = ex.InnerException;
        }

        if (errorCode == 0)
        {
            errorCode = 500;
        }

        string errMsg = ex.Message;
        if (request is not null)
        {
            errMsg = string.IsNullOrWhiteSpace( errMsg ) ? request.GetPropsAsStr() : $"{Environment.NewLine}{request}";
        }

        Log.Error( ex, errMsg );
        return new WebExceptionResult( errorCode, ex.Message );
    }
}
