using Microsoft.Extensions.DependencyInjection;

namespace SET.WebAPI.Controllers;

[Route( template: "api/habits")]
[ApiController]
[Authorize( AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme )]
public class HabitController : BaseController
{
    private readonly IHabitService m_habitService;

    public HabitController( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        m_habitService = serviceProvider.GetRequiredService<IHabitService>();
    }

    [HttpGet( template: "inprogress" )]
    public Task<IActionResult> InProgressIndex()
    {
        return TryCatchAsync( async ( user ) =>
            Ok( await m_habitService.GetInProgressAsync( user.Id ).DefaultConfigureAwait() ) );
    }

    [HttpGet( template: "archive" )]
    public Task<IActionResult> GetArchiveHabits()
    {
        return TryCatchAsync( async ( user ) =>
            Ok( await m_habitService.GetArchivedAsync( user.Id ).DefaultConfigureAwait() ) );
    }

    [HttpGet(template: "{habitId}")]
    public Task<IActionResult> Load( long habitId )
    {
        return TryCatchAsync( async ( User user ) =>
            ToActionResult( await m_habitService.GetForEditAsync( user.Id, habitId ).DefaultConfigureAwait() ) );
    }

    [HttpPost( template: "archivestatus" )]
    public Task<IActionResult> SetHabitArchiveStatus( [FromBody] HabitArchiveStatus habitArchiveStatus )
    {
        return TryCatchAsync( async ( User user ) =>
            ToActionResult( await m_habitService.SetArchiveStatusAsync( user.Id, habitArchiveStatus, RequestDeviceId ).DefaultConfigureAwait() ) );
    }

    [HttpPost( template: "{habitId}" )]
    public Task<IActionResult> Update( [FromBody] EditUserHabitDto habitDto )
    {
        return TryCatchAsync( async (User user) =>
            ToActionResult( await m_habitService.SaveAsync( user.Id, habitDto, RequestDeviceId ).DefaultConfigureAwait() ) );
    }

    [HttpPut("priorities")]
    public Task<IActionResult> ResetPrioritiesAsync( [FromBody] List<UserHabitWithPriority> habits )
    {
        return TryCatchAsync( async (user) =>
            ToActionResult( await m_habitService.ResetPrioritiesAsync( user.Id, habits ).DefaultConfigureAwait() ) );
    }

    [HttpDelete("{habitId}")]
    public Task<IActionResult> Delete( long habitId )
    {
        return TryCatchAsync( async ( User user ) =>
            ToActionResult( await m_habitService.DeleteAsync( user.Id, habitId, RequestDeviceId ).DefaultConfigureAwait() ) );
    }
}
