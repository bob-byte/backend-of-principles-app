using Microsoft.Extensions.DependencyInjection;

namespace SET.WebAPI.Controllers;

[Route( template: "api/reminder" )]
[ApiController]
[Authorize( AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme )]
public class ReminderController : BaseController
{
    private readonly IReminderService m_reminderService;

    public ReminderController( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        m_reminderService = serviceProvider.GetRequiredService<IReminderService>();
    }

    [HttpGet( "habitsreport" )]
    public Task<IActionResult> LoadReminder()
    {
        return TryCatchAsync( async ( user ) =>
            Ok( await m_reminderService.GetHabitsReportReminderAsync( user.Id ).DefaultConfigureAwait() ) );
    }

    [HttpGet( "all" )]
    public Task<IActionResult> LoadAllReminders()
    {
        return TryCatchAsync( async ( user ) =>
            Ok( await m_reminderService.GetAllRemindersAsync( user.Id ).DefaultConfigureAwait() ) );
    }

    [HttpPost( template: "habitsreport/{id}" )]
    public Task<IActionResult> SaveReminderAsync( [FromBody] UserReminderDto userReminder )
    {
        return TryCatchAsync( async ( User user ) =>
            ToActionResult( await m_reminderService.SaveHabitsReportReminderAsync( user, userReminder ).DefaultConfigureAwait() ) );
    }
}
