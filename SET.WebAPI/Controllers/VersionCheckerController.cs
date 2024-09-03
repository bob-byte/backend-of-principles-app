using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using SET.WebAPI.Resources.AppStrings;

using System.Globalization;

namespace SET.WebAPI.Controllers;

public class VersionCheckerController : BaseController
{
    private readonly IConfiguration m_configuration;
    public VersionCheckerController( IServiceProvider serviceProvider )
       : base( serviceProvider )
    {
        m_configuration = serviceProvider.GetRequiredService<IConfiguration>();
    }

    [HttpGet( "version/frontendlatest" )]
    public async Task<IActionResult> GetAppVersionAsync( [FromQuery] string language )
    {
        LatestVersionResponse latestVersionResponse = new LatestVersionResponse()
        {
            AppVersion = m_configuration["VersionSettings:LatestFrontendVersion"],
            VersionDescription = LocStrings.ResourceManager.GetString( "VersionDescription", CultureInfo.GetCultureInfo( language ) )
        };
        return Ok( latestVersionResponse );
    }
}
