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

    [HttpGet( "version/frontendlatestversion/{language}" )]
    public async Task<IActionResult> GetAppVersionAsync( string language )
    {
        LocStrings.Culture = CultureInfo.GetCultureInfo( language );
        LatestVersionResponse latestVersionResponse = new LatestVersionResponse()
        {
            AppVersion = m_configuration["VersionSettings:LatestVersion"],
            VersionDescription = LocStrings.VersionDescription
        };
        return Ok( new { latestVersionResponse } );
    }
}
