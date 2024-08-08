using AutoMapper;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
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
    }

    protected IServiceProvider ServiceProvider { get; }
    protected AppDbContext DbContext { get; }
    protected IMapper Mapper { get; }

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

    protected async Task<IActionResult> TryCatchAsync( long userId, Func<User, Task<IActionResult>> action )
    {
        User? user = await DbContext.Users.FindAsync( userId );

        IActionResult result;
        if (user == null)
        {
            result = BadRequest( error: "UserIsNotFound" );
        }
        else
        {
            try
            {
                result = await action(user);
            }
            catch (Exception ex)
            {
                result = WriteExceptionStatus( ex );
            }
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
