

namespace SET.WebAPI.Controllers;

[Route( template: "api/areasoflife" )]
[ApiController]
[Authorize( AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme )]
public class AreaOfLifeController : BaseController
{
    public AreaOfLifeController( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        //do nothing
    }

    [HttpGet]
    public Task<IActionResult> Index( [FromQuery] long userId )
    {
        return TryCatchAsync( userId, async ( user ) =>
        {
            List<UserAreaOfLife> userAreasOfLife = await DbContext.
                UserAreasOfLife.
                Where( u => u.UserId == userId ).
                ToListAsync();

            return Ok( userAreasOfLife );
        } );
    }
}

