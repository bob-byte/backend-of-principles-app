using SET.Shared.Models;

namespace SET.WebAPI.Controllers;

[Route( template: "api/reminder" )]
[ApiController]
[Authorize( AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme )]
public class ReminderController : BaseController
{
    public ReminderController( IServiceProvider serviceProvider )
    : base( serviceProvider )
    {
        //do nothing
    }

    [HttpGet( "habitsreport" )]
    public Task<IActionResult> LoadReminder()
    {
        return TryCatchAsync( async ( user ) =>
        {
            UserReminder? reminder = await DbContext.UserReminders
                .Where( r => r.UserId == user.Id && r.UserNotificationRequestId == 1 )
                .FirstOrDefaultAsync()
                .ConfigureAwait( false );

            UserReminderDto dto = Mapper.Map<UserReminderDto>( reminder ) ?? new UserReminderDto();

            return Ok( dto );
        } );
    }

    [HttpGet( "all" )]
    public Task<IActionResult> LoadAllReminders()
    {
        return TryCatchAsync( async ( user ) =>
        {
            List<UserReminder> generalReminders = await DbContext.UserReminders
                .Where( r => r.UserId == user.Id )
                .ToListAsync()
                .ConfigureAwait( false );

            List<UserHabitReminder> habitReminders = await DbContext.UserHabitReminders
                .Where( r => r.UserHabit.User.Id == user.Id )
                .Include( r => r.DaysOfWeek )
                .ToListAsync()
                .ConfigureAwait( false );

            List<UserReminderDto> reminderDtos = Mapper.Map<List<UserReminderDto>>( generalReminders ) ?? new List<UserReminderDto>();
            List<UserHabitReminderDto> habitReminderDtos = Mapper.Map<List<UserHabitReminderDto>>( habitReminders ) ?? new List<UserHabitReminderDto>();

            var result = new
            {
                GeneralReminders = reminderDtos,
                UserHabitReminders = habitReminderDtos
            };

            return Ok( result );
        } );
    }

    [HttpPost( template: "habitsreport/{id}" )]
    public Task<IActionResult> SaveReminderAsync( [FromBody] UserReminderDto userReminder )
    {
        return TryCatchAsync( async ( User user ) =>
        {
            #region check parameter
            if (userReminder is null)
            {
                return BadRequest( "HabitsReportReminderIsNullInSaveReminderEndpoint" );
            }
            #endregion

            UserReminder? reminder = await DbContext.UserReminders
                .Where( r => r.UserId == user.Id && r.UserNotificationRequestId == 1 )
                .FirstOrDefaultAsync()
                .ConfigureAwait( false );

            DateTime clientLastModified = NormalizeSyncTimestamp( userReminder.LastModified );
            if (reminder is not null)
            {
                DateTime serverLastModified = NormalizeStoredTimestamp( reminder.UpdatedAt, DateTime.UtcNow );
                if (IsClientTimestampOlder( userReminder.LastModified, serverLastModified ))
                {
                    return ConflictBecauseServerIsNewer( nameof( UserReminder ), reminder.Id, serverLastModified, userReminder.LastModified );
                }

                reminder.Title = userReminder.Title;
                reminder.Description = userReminder.Description;
                reminder.Time = userReminder.Time;
                reminder.IsEnabled = userReminder.IsEnabled;
                reminder.UpdatedAt = clientLastModified;
                DbContext.UserReminders.Update( reminder );
            }
            else
            {
                reminder = Mapper.Map<UserReminder>( userReminder );
                reminder.UserId = user.Id;
                reminder.User = null;
                reminder.UserNotificationRequestId = 1;
                reminder.UpdatedAt = clientLastModified;
                await DbContext.UserReminders.AddAsync( reminder ).ConfigureAwait( false );
            }

            await DbContext.SaveChangesAsync().DefaultConfigureAwait();

            var result = new
            {
                Id = reminder.Id,
                UserNotificationRequestId = reminder.UserNotificationRequestId
            };
            IActionResult actionResult = Ok( result );
            return actionResult;
        } );
    }
}
