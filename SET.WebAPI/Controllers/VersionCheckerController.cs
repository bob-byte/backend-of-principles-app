using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SET.WebAPI.Controllers;

public class VersionChecker : BaseController
{
    private readonly IConfiguration m_configuration;
    public VersionChecker( IServiceProvider serviceProvider )
       : base( serviceProvider )
    {
        m_configuration = serviceProvider.GetRequiredService<IConfiguration>();
    }

    [HttpGet( "versioncheck" )]
    public async Task<IActionResult> GetAppVersionAsync()
    {
        string appVersion = m_configuration["VersionSettings:CurrentVersion"];
        return Ok( appVersion );
    }
}
