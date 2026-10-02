using Microsoft.Extensions.DependencyInjection;

namespace SET.WebAPI.Controllers;

[Route( template: "api/areasoflife" )]
[ApiController]
[Authorize( AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme )]
public class AreaOfLifeController : BaseController
{
    private readonly IAreaOfLifeService m_areaOfLifeService;

    public AreaOfLifeController( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        m_areaOfLifeService = serviceProvider.GetRequiredService<IAreaOfLifeService>();
    }

    [HttpGet]
    public Task<IActionResult> Index()
    {
        return TryCatchAsync( async ( user ) =>
            Ok( await m_areaOfLifeService.GetAllAsync( user.Id ).DefaultConfigureAwait() ) );
    }
}
