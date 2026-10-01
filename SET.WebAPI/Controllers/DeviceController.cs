namespace SET.WebAPI.Controllers;

/// <summary>
/// Push-token registry for silent sync pushes to the user's other devices.
/// </summary>
[Route( "api/device" )]
[ApiController]
[Authorize( AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme )]
public class DeviceController : BaseController
{
    private const int MaxDeviceIdLength = 64;
    private const int MaxTokenLength = 512;
    private const int MaxPlatformLength = 16;

    public DeviceController( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
    }

    [HttpPut( "push-token" )]
    public Task<IActionResult> RegisterPushTokenAsync( [FromBody] RegisterPushTokenDto request )
    {
        return TryCatchAsync( async user =>
        {
            if (request is null)
            {
                return BadRequest( "RequestIsNull" );
            }

            string deviceId = request.DeviceId?.Trim() ?? string.Empty;
            if (deviceId.Length == 0 || deviceId.Length > MaxDeviceIdLength)
            {
                return BadRequest( "DeviceIdIsInvalid" );
            }

            string token = request.Token?.Trim() ?? string.Empty;
            if (token.Length == 0 || token.Length > MaxTokenLength)
            {
                return BadRequest( "TokenIsInvalid" );
            }

            string platform = (request.Platform ?? string.Empty).Trim().ToLowerInvariant();
            if (platform.Length > MaxPlatformLength)
            {
                platform = platform[..MaxPlatformLength];
            }

            // A reinstall or another account on the same install reuses the FCM token.
            List<UserDevice> sameToken = await DbContext.UserDevices
                .Where( d => d.PushToken == token && d.DeviceId != deviceId )
                .ToListAsync()
                .DefaultConfigureAwait();
            DbContext.UserDevices.RemoveRange( sameToken );

            DateTime now = DateTime.UtcNow;
            UserDevice? device = await DbContext.UserDevices
                .FirstOrDefaultAsync( d => d.DeviceId == deviceId )
                .DefaultConfigureAwait();
            if (device is null)
            {
                device = new UserDevice { DeviceId = deviceId, CreatedAt = now };
                DbContext.UserDevices.Add( device );
            }

            device.UserId = user.Id;
            device.PushToken = token;
            device.Platform = platform;
            device.UpdatedAt = now;

            await DbContext.SaveChangesAsync().DefaultConfigureAwait();
            return NoContent();
        }, request );
    }

    [HttpDelete( "push-token/{deviceId}" )]
    public Task<IActionResult> UnregisterPushTokenAsync( string deviceId )
    {
        return TryCatchAsync( async user =>
        {
            List<UserDevice> devices = await DbContext.UserDevices
                .Where( d => d.DeviceId == deviceId && d.UserId == user.Id )
                .ToListAsync()
                .DefaultConfigureAwait();
            if (devices.Count > 0)
            {
                DbContext.UserDevices.RemoveRange( devices );
                await DbContext.SaveChangesAsync().DefaultConfigureAwait();
            }

            return NoContent();
        } );
    }
}
