using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SET.Shared.Models;

using System;
using System.Linq;
using System.Reflection;

namespace SET.WebAPI.Controllers;

[Route("api/profile")]
[ApiController]
[Authorize( AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme )]
public class ProfileController : BaseController
{
    public ProfileController(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        //do nothing
    }

    [HttpGet( template: "{userId}" )]
    public Task<IActionResult> LoadAsync(long userId)
    {
        return TryCatchAsync( userId, ( User user ) =>
        {
            Profile data = Mapper.Map<Profile>( user );
            IActionResult actionResult = Ok( data );
            return Task.FromResult( actionResult );
        } );
    }

    [HttpPut( template: "name/{userId}" )]
    public Task<IActionResult> SaveNameAsync( long userId, [FromBody] string name )
    {
        return TryCatchAsync( userId, async ( User user ) =>
        {
            user.Name = name;
            DbContext.Users.Update( user );

            await DbContext.SaveChangesAsync();
            IActionResult actionResult = Ok();
            return actionResult;
        } );
    }

    [HttpPut( template: "mainslogan/{userId}" )]
    public Task<IActionResult> SaveMainSloganAsync( long userId, [FromBody] string mainSlogan )
    {
        return TryCatchAsync( userId, async ( User user ) =>
        {
            user.MainSlogan = mainSlogan;
            DbContext.Users.Update( user );

            await DbContext.SaveChangesAsync();
            IActionResult actionResult = Ok();
            return actionResult;
        } );
    }

    [HttpPut( template: "mission/{userId}" )]
    public Task<IActionResult> SaveMissionAsync( long userId, [FromBody] string mission )
    {
        return TryCatchAsync( userId, async ( User user ) =>
        {
            user.Mission = mission;
            DbContext.Users.Update( user );

            await DbContext.SaveChangesAsync();
            IActionResult actionResult = Ok();
            return actionResult;
        } );
    }
}
