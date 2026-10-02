using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using SET.WebAPI.Models;
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
    public IActionResult GetLatestClientAppVersionAsStr( [FromQuery] string language, [FromQuery] string? osPlatform )
    {
        return TryCatch( () =>
        {
            var appClientCulture = CultureInfo.GetCultureInfo( language );

            bool isClientAppAndroid = osPlatform is null || osPlatform.ToLower().Contains( "android" );
            string pathToLocalizedVersionDescr = isClientAppAndroid ? "AndroidVersionDescription" : "IosVersionDescription";

            LatestVersionResponse latestVersionResponse = new()
            {
                AppVersion = isClientAppAndroid ? m_configuration["ClientVersions:Android"] : m_configuration["ClientVersions:iOS"],
                VersionDescription = LocStrings.ResourceManager.GetString( pathToLocalizedVersionDescr, appClientCulture )
            };
            return Ok( latestVersionResponse );
        } );
    }
}
