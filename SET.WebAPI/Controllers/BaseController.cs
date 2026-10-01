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
        DbContext = ServiceProvider.GetRequiredService<AppDbContext>();
        Mapper = ServiceProvider.GetRequiredService<IMapper>();
        JwtTokenService = ServiceProvider.GetRequiredService<IJwtTokenService>();
    }

    public const string DeviceIdHeader = "X-Device-Id";

    protected IServiceProvider ServiceProvider { get; }
    protected AppDbContext DbContext { get; }
    protected IMapper Mapper { get; }
    protected IJwtTokenService JwtTokenService { get; }

    /// <summary>Per-install id the Flutter client sends on every request (null for MAUI / web).</summary>
    protected string? RequestDeviceId
    {
        get
        {
            string? value = Request?.Headers[DeviceIdHeader].FirstOrDefault()?.Trim();
            return string.IsNullOrEmpty( value ) || value.Length > 64 ? null : value;
        }
    }

    /// <summary>
    /// Wakes the user's other devices so they sync and fix local reminders. Call after the
    /// change is committed.
    /// </summary>
    protected void NotifyOtherDevices(
        long userId,
        IEnumerable<long>? deletedTaskIds = null,
        IEnumerable<long>? deletedHabitIds = null )
    {
        ServiceProvider.GetService<ISyncPushService>()?.Enqueue( new SyncPushRequest(
            userId,
            RequestDeviceId,
            deletedTaskIds?.ToArray() ?? Array.Empty<long>(),
            deletedHabitIds?.ToArray() ?? Array.Empty<long>() ) );
    }

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
            result = await action().DefaultConfigureAwait();
        }
        catch (Exception ex)
        {
            result = WriteExceptionStatus( ex, request );
        }

        return result;
    }

    protected async Task<IActionResult> TryCatch( Func<User, IActionResult> action, object? request = null )
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
                result = action( user );
            }
        }
        catch (Exception ex)
        {
            result = WriteExceptionStatus( ex, request );
        }

        return result;
    }

    protected IActionResult TryCatch( Func<IActionResult> action, object? request = null )
    {
        IActionResult result;
        try
        {
            result = action();
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
