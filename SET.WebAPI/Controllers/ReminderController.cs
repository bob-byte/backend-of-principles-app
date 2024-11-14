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

    [HttpGet( "generalreminder" )]
    public Task<IActionResult> LoadReminder()
    {
        return TryCatchAsync( async ( user ) =>
        {
            List<UserReminder> reminders = await DbContext.UserReminders
                .Where( r => r.UserId == user.Id )
                .ToListAsync()
                .ConfigureAwait( false );

            List<UserReminderDto> reminderDtos = Mapper.Map<List<UserReminderDto>>( reminders );

            return Ok( reminderDtos );
        } );
    }

    [HttpGet( "allreminder" )]
    public Task<IActionResult> LoadAllReminders( )
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



    [HttpPut( template: "habitsreportreminder" )]
    public Task<IActionResult> SaveReminderAsync( [FromBody] UserReminderDto userReminder )
    {
        return TryCatchAsync( async ( User user ) =>
        {
            user.HabitsReportReminder = Mapper.Map<UserReminder>( userReminder );
            DbContext.Users.Update( user );

            await DbContext.SaveChangesAsync().DefaultConfigureAwait();
            IActionResult actionResult = Ok();
            return actionResult;
        } );
    }
}
