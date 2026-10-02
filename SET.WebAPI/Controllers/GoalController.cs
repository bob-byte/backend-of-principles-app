using Microsoft.Extensions.DependencyInjection;

namespace SET.WebAPI.Controllers;

[Route( template: "api/goals" )]
[ApiController]
[Authorize( AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme )]
public class GoalController : BaseController
{
    private readonly IGoalService m_goalService;

    public GoalController( IServiceProvider serviceProvider )
    : base( serviceProvider )
    {
        m_goalService = serviceProvider.GetRequiredService<IGoalService>();
    }

    [HttpGet]
    public Task<IActionResult> Index( )
    {
        return TryCatchAsync( async (user) =>
            Ok( await m_goalService.GetActiveAsync( user.Id ).DefaultConfigureAwait() ) );
    }

    [HttpGet( template: "archive" )]
    public Task<IActionResult> GetArchiveGoals()
    {
        return TryCatchAsync( async ( user ) =>
            Ok( await m_goalService.GetArchivedAsync( user.Id ).DefaultConfigureAwait() ) );
    }

    [HttpPost( template: "archivestatus" )]
    public Task<IActionResult> SetGoalArchiveStatus( [FromBody] GoalArchiveStatus goalArchiveStatus )
    {
        return TryCatchAsync( async () =>
            ToActionResult( await m_goalService.SetArchiveStatusAsync( goalArchiveStatus, RequestDeviceId ).DefaultConfigureAwait() ) );
    }

    [HttpDelete( "{goalId}" )]
    public Task<IActionResult> Delete( long goalId )
    {
        return TryCatchAsync( async () =>
            ToActionResult( await m_goalService.DeleteAsync( goalId ).DefaultConfigureAwait() ) );
    }

    [HttpPost(template: "{goalId}")]
    public Task<IActionResult> Save( [FromBody] UserGoalDto userGoal)
    {
        return TryCatchAsync( async (User user) =>
            ToActionResult( await m_goalService.SaveAsync( user, userGoal ).DefaultConfigureAwait() ) );
    }
}
