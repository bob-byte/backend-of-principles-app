using Microsoft.Extensions.DependencyInjection;

namespace SET.WebAPI.Controllers;

[Route( "api/sync" )]
[ApiController]
[Authorize( AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme )]
public class SyncController : BaseController
{
    private readonly ISyncService m_syncService;

    public SyncController( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        m_syncService = serviceProvider.GetRequiredService<ISyncService>();
    }

    [HttpGet( "ping" )]
    public Task<IActionResult> PingAsync()
    {
        return TryCatchAsync( _ => Task.FromResult<IActionResult>( Ok() ) );
    }

    [HttpGet( "bootstrap" )]
    public Task<IActionResult> BootstrapAsync()
    {
        return TryCatchAsync( async user =>
            Ok( await m_syncService.GetBootstrapAsync( user ).DefaultConfigureAwait() ) );
    }

    /// <summary>
    /// Incremental catch-up since the client's last successful sync cursor.
    /// Full bootstrap remains available for first sign-in / cold start.
    /// </summary>
    [HttpGet( "changes" )]
    public Task<IActionResult> ChangesAsync( [FromQuery] DateTime? since )
    {
        return TryCatchAsync( async user =>
            Ok( await m_syncService.GetChangesAsync( user, since ).DefaultConfigureAwait() ) );
    }
}
