using Microsoft.Extensions.DependencyInjection;

namespace SET.WebAPI.Controllers;

[Route("api/profile")]
[ApiController]
[Authorize( AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme )]
public class ProfileController : BaseController
{
    private readonly IProfileService m_profileService;

    public ProfileController(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        m_profileService = serviceProvider.GetRequiredService<IProfileService>();
    }

    [HttpGet]
    public Task<IActionResult> LoadAsync()
    {
        return TryCatchAsync( ( User user ) =>
            Task.FromResult<IActionResult>( Ok( m_profileService.GetProfile( user ) ) ) );
    }

    [HttpPut( template: "name" )]
    public Task<IActionResult> SaveNameAsync( [FromBody] string userName )
    {
        return TryCatchAsync( async ( User user ) =>
            ToActionResult( await m_profileService.SaveNameAsync( user, userName ).DefaultConfigureAwait() ) );
    }

    [HttpPut( template: "mainslogan" )]
    public Task<IActionResult> SaveMainSloganAsync( [FromBody] string mainSlogan )
    {
        return TryCatchAsync( async ( User user ) =>
        {
            await m_profileService.SaveMainSloganAsync( user, mainSlogan ).DefaultConfigureAwait();
            return Ok();
        } );
    }

    [HttpPut( template: "gender" )]
    public Task<IActionResult> SaveGenderAsync( [FromBody] Gender gender )
    {
        return TryCatchAsync( async ( User user ) =>
            ToActionResult( await m_profileService.SaveGenderAsync( user, gender ).DefaultConfigureAwait() ) );
    }

    [HttpPut( template: "roadguide" )]
    public Task<IActionResult> SaveHasSeenRoadGuideAsync( [FromBody] bool hasSeenRoadGuide )
    {
        return TryCatchAsync( async ( User user ) =>
        {
            await m_profileService.SaveHasSeenRoadGuideAsync( user, hasSeenRoadGuide ).DefaultConfigureAwait();
            return Ok();
        } );
    }

    [HttpPut( template: "mission" )]
    public Task<IActionResult> SaveMissionAsync( [FromBody] string mission )
    {
        return TryCatchAsync( async ( User user ) =>
        {
            await m_profileService.SaveMissionAsync( user, mission ).DefaultConfigureAwait();
            return Ok();
        } );
    }
}
