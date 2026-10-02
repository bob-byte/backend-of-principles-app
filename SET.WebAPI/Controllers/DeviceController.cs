using Microsoft.Extensions.DependencyInjection;

namespace SET.WebAPI.Controllers;

/// <summary>
/// Push-token registry for silent sync pushes to the user's other devices.
/// </summary>
[Route( "api/device" )]
[ApiController]
[Authorize( AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme )]
public class DeviceController : BaseController
{
    private readonly IDeviceService m_deviceService;

    public DeviceController( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        m_deviceService = serviceProvider.GetRequiredService<IDeviceService>();
    }

    [HttpPut( "push-token" )]
    public Task<IActionResult> RegisterPushTokenAsync( [FromBody] RegisterPushTokenDto request )
    {
        return TryCatchAsync( async user =>
        {
            ServiceResult result = await m_deviceService.RegisterPushTokenAsync( user.Id, request ).DefaultConfigureAwait();
            return ToActionResult( result, NoContent );
        }, request );
    }

    [HttpDelete( "push-token/{deviceId}" )]
    public Task<IActionResult> UnregisterPushTokenAsync( string deviceId )
    {
        return TryCatchAsync( async user =>
        {
            await m_deviceService.UnregisterPushTokenAsync( user.Id, deviceId ).DefaultConfigureAwait();
            return NoContent();
        } );
    }
}
