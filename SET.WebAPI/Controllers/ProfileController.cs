using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SET.Shared.Models;

using System;
using System.Linq;
using System.Reflection;

namespace SET.WebAPI.Controllers;

[Route( "api/profile" )]
[ApiController]
[Authorize( AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme )]
public class ProfileController : BaseController
{
    public ProfileController( IServiceProvider serviceProvider )
        : base( serviceProvider )
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
    public Task<IActionResult> SaveNameAsync( [FromBody] SaveUserNameRequest request )
    {
        return TryCatchAsync( async ( User user ) =>
        {
            if (request is null)
            {
                return BadRequest( "SaveUserNameRequestIsNull" );
            }

            DateTime serverLastModified = NormalizeStoredTimestamp( user.UpdatedAt, user.CreatedAt );
            if (IsClientTimestampOlder( request.LastModified, serverLastModified ))
            {
                return ConflictBecauseServerIsNewer( nameof( User ), user.Id, serverLastModified, request.LastModified );
            }

            user.Name = request.UserName;
            user.UpdatedAt = NormalizeSyncTimestamp( request.LastModified );
            DbContext.Users.Update( user );

            await DbContext.SaveChangesAsync().DefaultConfigureAwait();
            IActionResult actionResult = Ok();
            return actionResult;
        } );
    }

    [HttpPut( template: "mainslogan" )]
    public Task<IActionResult> SaveMainSloganAsync( [FromBody] SaveUserMainSloganRequest request )
    {
        return TryCatchAsync( async ( User user ) =>
        {
            if (request is null)
            {
                return BadRequest( "SaveUserMainSloganRequestIsNull" );
            }

            DateTime serverLastModified = NormalizeStoredTimestamp( user.UpdatedAt, user.CreatedAt );
            if (IsClientTimestampOlder( request.LastModified, serverLastModified ))
            {
                return ConflictBecauseServerIsNewer( nameof( User ), user.Id, serverLastModified, request.LastModified );
            }

            user.MainSlogan = request.MainSlogan;
            user.UpdatedAt = NormalizeSyncTimestamp( request.LastModified );
            DbContext.Users.Update( user );

            await DbContext.SaveChangesAsync().DefaultConfigureAwait();
            IActionResult actionResult = Ok();
            return actionResult;
        } );
    }

    [HttpPut( template: "mission" )]
    public Task<IActionResult> SaveMissionAsync( [FromBody] SaveUserMissionRequest request )
    {
        return TryCatchAsync( async ( User user ) =>
        {
            if (request is null)
            {
                return BadRequest( "SaveUserMissionRequestIsNull" );
            }

            DateTime serverLastModified = NormalizeStoredTimestamp( user.UpdatedAt, user.CreatedAt );
            if (IsClientTimestampOlder( request.LastModified, serverLastModified ))
            {
                return ConflictBecauseServerIsNewer( nameof( User ), user.Id, serverLastModified, request.LastModified );
            }

            string? mission = request.Mission;
            string? oldMission = (string)user.Mission?.Clone();
            user.Mission = mission;
            user.UpdatedAt = NormalizeSyncTimestamp( request.LastModified );

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

                        reminder.UpdatedAt = user.UpdatedAt;
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
