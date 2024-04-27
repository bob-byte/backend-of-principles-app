using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SET.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TestController : ControllerBase
{
    [HttpGet]
    public IActionResult Test()
    {
        return Ok( "Api works" );
    }

    [Authorize( AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme )]
    [HttpGet( "auth" )]
    public IActionResult TestAuth()
    {
        return Ok( "Autorized" );
    }
}
