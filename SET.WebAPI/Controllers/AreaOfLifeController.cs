
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
    public Task<IActionResult> Index()
    {
        return TryCatchAsync( async ( user ) =>
        {
            List<UserAreaOfLife> userAreasOfLife = await DbContext.
                UserAreasOfLife.
                Where( u => u.UserId == user.Id ).
                ToListAsync();

            List<UserAreaOfLifeDto> result = Mapper.Map<List<UserAreaOfLifeDto>>( userAreasOfLife );
            return Ok( result );
        } );
    }
}

