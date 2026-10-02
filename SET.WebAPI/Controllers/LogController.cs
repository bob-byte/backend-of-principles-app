using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace SET.WebAPI.Controllers;

[Route( template: "api/logs" )]
[ApiController]
public class LogController : BaseController
{
    private readonly IClientLogService m_clientLogService;

    public LogController( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        m_clientLogService = serviceProvider.GetRequiredService<IClientLogService>();
    }

    [HttpPost]
    public Task<IActionResult> LogToServerConsole( [FromBody] SaveLogRequest saveLogRequest )
    {
        return TryCatchAsync( async () =>
        {
            long? userId;
            try
            {
                string? accessToken = await HttpContext.GetTokenAsync( tokenName: "access_token" ).DefaultConfigureAwait();
                userId = JwtTokenService.GetUserIdFromJwt( accessToken );
            }
            catch
            {
                userId = null;
            }

            ServiceResult result = await m_clientLogService.WriteAsync( saveLogRequest, userId ).DefaultConfigureAwait();
            return ToActionResult( result );
        } );
    }
}
