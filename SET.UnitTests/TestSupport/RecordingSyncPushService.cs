using BusinessLogic;

namespace SET.UnitTests.TestSupport;

internal sealed class RecordingSyncPushService : ISyncPushService
{
    public List<SyncPushRequest> Requests { get; } = new();

    public void Enqueue( SyncPushRequest request ) => Requests.Add( request );
}
