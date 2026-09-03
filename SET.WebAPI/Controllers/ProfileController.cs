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
            data.Id = user.Id;
            data.LastModified = user.CreatedAt;
            IActionResult actionResult = Ok( data );
            return Task.FromResult( actionResult );
        } );
    }

    [HttpPut( template: "name" )]
    public Task<IActionResult> SaveNameAsync( [FromBody] string userName )
    {
        return TryCatchAsync( async ( User user ) =>
        {
            if (string.IsNullOrWhiteSpace( userName ))
            {
                return BadRequest( "UserNameIsNullOrWhiteSpace" );
            }

            user.Name = userName;
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
            string? oldMission = (string)user.Mission?.Clone();
            user.Mission = mission;

            if (oldMission is not null)
            {
                List<UserHabitReminder> remindersToUpdate = await DbContext.UserHabitReminders.
                    Include( r => r.UserHabit ).
                    Where( r => r.Title == oldMission && r.UserHabit.UserId == user.Id ).
                    ToListAsync().
                    DefaultConfigureAwait();
                List<UserReminder> userReminders =
                    await DbContext.UserReminders.Where( r => r.UserId == user.Id && (r.Title == oldMission || r.Description == oldMission) ).ToListAsync().DefaultConfigureAwait();

                if (remindersToUpdate?.Count > 0)
                {
                    foreach (UserHabitReminder reminder in remindersToUpdate)
                    {
                        reminder.Title = mission;
                    }

                    DbContext.UserHabitReminders.UpdateRange( remindersToUpdate );
                }

                if (userReminders?.Count > 0)
                {
                    foreach (UserReminder reminder in userReminders)
                    {
                        if (reminder.Title == oldMission)
                        {
                            reminder.Title = mission;
                        }

                        if (reminder.Description == oldMission)
                        {
                            reminder.Description = mission;
                        }
                    }
                
                    DbContext.UserReminders.UpdateRange( userReminders );
                }
            }

            DbContext.Users.Update( user );

            await DbContext.SaveChangesAsync().DefaultConfigureAwait();
            IActionResult actionResult = Ok();
            return actionResult;
        } );
    }
}
