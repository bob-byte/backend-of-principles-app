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
                .Where( r => r.UserId == user.Id )
                .FirstOrDefaultAsync()
                .ConfigureAwait( false );

            UserReminderDto reminderDto = reminder is null ? null : Mapper.Map<UserReminderDto>( reminder );

            return Ok( reminderDto );
        } );
    }

    [HttpGet( "all" )]
    public Task<IActionResult> LoadAllReminders()
    {
        return TryCatchAsync( async (user) =>
        {
            List<UserReminder> generalReminders = await DbContext.UserReminders
                .Where( r => r.UserId == user.Id )
                .ToListAsync();

            List<UserHabitReminder> habitReminders = await DbContext.UserHabitReminders
                .Where( r => r.UserHabit.User.Id == user.Id )
                .Include( r => r.DaysOfWeek )
                .ToListAsync();

            List<UserReminderDto> reminderDtos = Mapper.Map<List<UserReminderDto>>( generalReminders );
            List<UserHabitReminderDto> habitReminderDtos = Mapper.Map<List<UserHabitReminderDto>>( habitReminders );

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
            
            user.HabitsReportReminder = Mapper.Map<UserReminder>( userReminder );
            await DbContext.Users.AddOrUpdateAsync( user ).DefaultConfigureAwait();

            await DbContext.SaveChangesAsync().DefaultConfigureAwait();

            var result = new { Id = user.HabitsReportReminder!.Id };
            IActionResult actionResult = Ok( result );
            return actionResult;
        } );
    }
}
