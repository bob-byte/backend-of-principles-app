namespace BusinessLogic;

/// <summary>
/// Push-token registry for silent sync pushes to the user's other devices.
/// </summary>
public interface IDeviceService
{
    Task<ServiceResult> RegisterPushTokenAsync( long userId, RegisterPushTokenDto request );

    Task UnregisterPushTokenAsync( long userId, string deviceId );
}
