using BusinessLogic;
using SET.UnitTests.TestSupport;

namespace SET.UnitTests.Services;

public class SyncPushServiceExtensionsTests
{
    [Fact]
    public void NotifyOtherDevices_DefaultArgs_EnqueuesEmptyDeletedLists()
    {
        RecordingSyncPushService push = new();

        push.NotifyOtherDevices( 7, "device-a" );

        SyncPushRequest request = Assert.Single( push.Requests );
        Assert.Equal( 7, request.UserId );
        Assert.Equal( "device-a", request.OriginDeviceId );
        Assert.Empty( request.DeletedTaskIds );
        Assert.Empty( request.DeletedHabitIds );
    }

    [Fact]
    public void NotifyOtherDevices_DeletedIds_PassesThrough()
    {
        RecordingSyncPushService push = new();

        push.NotifyOtherDevices( 7, null, deletedTaskIds: new[] { 1L, 2L }, deletedHabitIds: new[] { 3L } );

        SyncPushRequest request = Assert.Single( push.Requests );
        Assert.Null( request.OriginDeviceId );
        Assert.Equal( new[] { 1L, 2L }, request.DeletedTaskIds );
        Assert.Equal( new[] { 3L }, request.DeletedHabitIds );
    }
}
