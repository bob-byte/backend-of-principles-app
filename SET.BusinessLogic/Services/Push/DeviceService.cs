namespace BusinessLogic;

public class DeviceService : IDeviceService
{
    private const int MaxDeviceIdLength = 64;
    private const int MaxTokenLength = 512;
    private const int MaxPlatformLength = 16;

    private readonly AppDbContext m_dbContext;

    public DeviceService( AppDbContext dbContext )
    {
        m_dbContext = dbContext;
    }

    public async Task<ServiceResult> RegisterPushTokenAsync( long userId, RegisterPushTokenDto request )
    {
        if (request is null)
        {
            return ServiceError.BadRequest( "RequestIsNull" );
        }

        string deviceId = request.DeviceId?.Trim() ?? string.Empty;
        if (deviceId.Length == 0 || deviceId.Length > MaxDeviceIdLength)
        {
            return ServiceError.BadRequest( "DeviceIdIsInvalid" );
        }

        string token = request.Token?.Trim() ?? string.Empty;
        if (token.Length == 0 || token.Length > MaxTokenLength)
        {
            return ServiceError.BadRequest( "TokenIsInvalid" );
        }

        string platform = (request.Platform ?? string.Empty).Trim().ToLowerInvariant();
        if (platform.Length > MaxPlatformLength)
        {
            platform = platform[..MaxPlatformLength];
        }

        // A reinstall or another account on the same install reuses the FCM token.
        List<UserDevice> sameToken = await m_dbContext.UserDevices
            .Where( d => d.PushToken == token && d.DeviceId != deviceId )
            .ToListAsync()
            .DefaultConfigureAwait();
        m_dbContext.UserDevices.RemoveRange( sameToken );

        DateTime now = DateTime.UtcNow;
        UserDevice? device = await m_dbContext.UserDevices
            .FirstOrDefaultAsync( d => d.DeviceId == deviceId )
            .DefaultConfigureAwait();
        if (device is null)
        {
            device = new UserDevice { DeviceId = deviceId, CreatedAt = now };
            m_dbContext.UserDevices.Add( device );
        }

        device.UserId = userId;
        device.PushToken = token;
        device.Platform = platform;
        device.UpdatedAt = now;

        await m_dbContext.SaveChangesAsync().DefaultConfigureAwait();
        return ServiceResult.Success;
    }

    public async Task UnregisterPushTokenAsync( long userId, string deviceId )
    {
        List<UserDevice> devices = await m_dbContext.UserDevices
            .Where( d => d.DeviceId == deviceId && d.UserId == userId )
            .ToListAsync()
            .DefaultConfigureAwait();
        if (devices.Count > 0)
        {
            m_dbContext.UserDevices.RemoveRange( devices );
            await m_dbContext.SaveChangesAsync().DefaultConfigureAwait();
        }
    }
}
