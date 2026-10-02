using BusinessLogic;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
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
        JwtTokenService = ServiceProvider.GetRequiredService<IJwtTokenService>();
    }

    public const string DeviceIdHeader = "X-Device-Id";

    protected IServiceProvider ServiceProvider { get; }
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

    protected IActionResult ToActionResult( ServiceResult result )
    {
        return ToActionResult( result, Ok );
    }

    protected IActionResult ToActionResult<T>( ServiceResult<T> result )
    {
        return ToActionResult( result, () => Ok( result.Value ) );
    }

    protected IActionResult ToActionResult( ServiceResult result, Func<IActionResult> onSuccess )
    {
        return result.Error is { } error
            ? StatusCode( error.StatusCode, error.Body )
            : onSuccess();
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

            User? user = await ServiceProvider.GetRequiredService<IAccountService>()
                .FindUserAsync( userId )
                .DefaultConfigureAwait();

            result = user is null
                ? BadRequest( error: "UserIsNotFound" )
                : await action( user ).DefaultConfigureAwait();
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
