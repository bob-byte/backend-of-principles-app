using BusinessLogic;
using BusinessLogic.Models;
using Microsoft.EntityFrameworkCore;
using SET.DataAccess;
using SET.Shared.Models;
using SET.UnitTests.TestSupport;

namespace SET.UnitTests.Services;

public class DeviceServiceTests
{
    private static (DeviceService service, AppDbContext db) CreateSut()
    {
        AppDbContext db = TestDb.Create();
        return (new DeviceService( db ), db);
    }

    private static async Task AddDeviceAsync( AppDbContext db, long userId, string deviceId, string token )
    {
        db.UserDevices.Add( new UserDevice
        {
            UserId = userId,
            DeviceId = deviceId,
            PushToken = token,
            Platform = "ios",
            CreatedAt = new DateTime( 2026, 1, 1, 0, 0, 0, DateTimeKind.Utc ),
            UpdatedAt = new DateTime( 2026, 1, 1, 0, 0, 0, DateTimeKind.Utc ),
        } );
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task RegisterPushTokenAsync_rejects_null_request()
    {
        (DeviceService service, _) = CreateSut();

        TestData.AssertError( await service.RegisterPushTokenAsync( 1, null! ), 400, "RequestIsNull" );
    }

    [Theory]
    [InlineData( "", "token", "DeviceIdIsInvalid" )]
    [InlineData( "   ", "token", "DeviceIdIsInvalid" )]
    [InlineData( "dev", "", "TokenIsInvalid" )]
    [InlineData( "dev", "  ", "TokenIsInvalid" )]
    public async Task RegisterPushTokenAsync_rejects_blank_fields( string deviceId, string token, string error )
    {
        (DeviceService service, AppDbContext db) = CreateSut();

        ServiceResult result = await service.RegisterPushTokenAsync( 1, new RegisterPushTokenDto { DeviceId = deviceId, Token = token } );

        TestData.AssertError( result, 400, error );
        Assert.Empty( db.UserDevices );
    }

    [Fact]
    public async Task RegisterPushTokenAsync_rejects_oversized_fields()
    {
        (DeviceService service, _) = CreateSut();

        TestData.AssertError(
            await service.RegisterPushTokenAsync( 1, new RegisterPushTokenDto { DeviceId = new string( 'd', 65 ), Token = "t" } ),
            400,
            "DeviceIdIsInvalid" );
        TestData.AssertError(
            await service.RegisterPushTokenAsync( 1, new RegisterPushTokenDto { DeviceId = "dev", Token = new string( 't', 513 ) } ),
            400,
            "TokenIsInvalid" );
    }

    [Fact]
    public async Task RegisterPushTokenAsync_creates_device_with_trimmed_lowercase_platform()
    {
        (DeviceService service, AppDbContext db) = CreateSut();

        ServiceResult result = await service.RegisterPushTokenAsync( 3, new RegisterPushTokenDto
        {
            DeviceId = " dev-1 ",
            Token = " tok ",
            Platform = " AndroidPlatformWithLongName ",
        } );

        Assert.True( result.IsSuccess );
        UserDevice device = await db.UserDevices.SingleAsync();
        Assert.Equal( 3, device.UserId );
        Assert.Equal( "dev-1", device.DeviceId );
        Assert.Equal( "tok", device.PushToken );
        Assert.Equal( "androidplatformw", device.Platform );
    }

    [Fact]
    public async Task RegisterPushTokenAsync_reassigns_existing_device_and_drops_stale_token_owners()
    {
        (DeviceService service, AppDbContext db) = CreateSut();
        await AddDeviceAsync( db, 1, "dev-1", "old-token" );
        await AddDeviceAsync( db, 2, "dev-old-install", "shared-token" );

        await service.RegisterPushTokenAsync( 5, new RegisterPushTokenDto
        {
            DeviceId = "dev-1",
            Token = "shared-token",
            Platform = "macos",
        } );

        UserDevice device = await db.UserDevices.SingleAsync();
        Assert.Equal( "dev-1", device.DeviceId );
        Assert.Equal( 5, device.UserId );
        Assert.Equal( "shared-token", device.PushToken );
        Assert.Equal( "macos", device.Platform );
    }

    [Fact]
    public async Task UnregisterPushTokenAsync_removes_only_the_users_device()
    {
        (DeviceService service, AppDbContext db) = CreateSut();
        await AddDeviceAsync( db, 1, "dev-1", "a" );
        await AddDeviceAsync( db, 2, "dev-2", "b" );

        await service.UnregisterPushTokenAsync( 1, "dev-2" );
        Assert.Equal( 2, await db.UserDevices.CountAsync() );

        await service.UnregisterPushTokenAsync( 1, "dev-1" );
        Assert.Equal( "dev-2", (await db.UserDevices.SingleAsync()).DeviceId );
    }
}
