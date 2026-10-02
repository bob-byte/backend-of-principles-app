using Microsoft.Extensions.DependencyInjection;

namespace SET.WebAPI.Controllers;

[Route( "api/progressesofhabit" )]
[ApiController]
[Authorize( AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme )]
public class ProgressOfHabitController : BaseController
{
    private readonly IHabitProgressService m_habitProgressService;

    public ProgressOfHabitController( IServiceProvider serviceProvider ) : base( serviceProvider )
    {
        m_habitProgressService = serviceProvider.GetRequiredService<IHabitProgressService>();
    }

    [HttpPost( template: "{progressId}" )]
    public Task<IActionResult> UpdateProgress( [FromBody] UpdateProgressDto progressDto )
    {
        return TryCatchAsync( async ( User user ) =>
            ToActionResult( await m_habitProgressService.UpdateProgressAsync( user.Id, progressDto ).DefaultConfigureAwait() ) );
    }
}
