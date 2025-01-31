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

    [HttpGet]
    public Task<IActionResult> LoadAsync()
    {
        return TryCatchAsync( ( User user ) =>
        {
            Profile data = Mapper.Map<Profile>( user );
            IActionResult actionResult = Ok( data );
            return Task.FromResult( actionResult );
        } );
    }

    [HttpPut( template: "name" )]
    public Task<IActionResult> SaveNameAsync( [FromBody] string name )
    {
        return TryCatchAsync( async ( User user ) =>
        {
            user.Name = name;
            DbContext.Users.Update( user );

            await DbContext.SaveChangesAsync().DefaultConfigureAwait();
            IActionResult actionResult = Ok();
            return actionResult;
        } );
    }

    [HttpPut( template: "mainslogan" )]
    public Task<IActionResult> SaveMainSloganAsync( [FromBody] string mainSlogan )
    {
        return TryCatchAsync( async ( User user ) =>
        {
            user.MainSlogan = mainSlogan;
            DbContext.Users.Update( user );

            await DbContext.SaveChangesAsync().DefaultConfigureAwait();
            IActionResult actionResult = Ok();
            return actionResult;
        } );
    }

    [HttpPut( template: "mission" )]
    public Task<IActionResult> SaveMissionAsync( [FromBody] string mission )
    {
        return TryCatchAsync( async ( User user ) =>
        {
            List<UserHabitReminder> remindersToUpdate = await DbContext.UserHabitReminders.
                    Where( h => h.Title == user.Mission && h.UserHabit.User.Id == user.Id ).
                    ToListAsync().
                    DefaultConfigureAwait();
            user.Mission = mission;

            if (remindersToUpdate?.Count > 0)
            {
                foreach (UserHabitReminder reminder in remindersToUpdate)
                {
                    reminder.Title = mission;
                }

                DbContext.UserHabitReminders.UpdateRange( remindersToUpdate );
            }

            DbContext.Users.Update( user );

            await DbContext.SaveChangesAsync().DefaultConfigureAwait();
            IActionResult actionResult = Ok();
            return actionResult;
        } );
    }
}
