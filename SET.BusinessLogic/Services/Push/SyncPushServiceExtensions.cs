namespace BusinessLogic;

public static class SyncPushServiceExtensions
{
    /// <summary>
    /// Wakes the user's other devices so they sync and fix local reminders. Call after the
    /// change is committed.
    /// </summary>
    public static void NotifyOtherDevices(
        this ISyncPushService pushService,
        long userId,
        string? originDeviceId,
        IEnumerable<long>? deletedTaskIds = null,
        IEnumerable<long>? deletedHabitIds = null )
    {
        pushService.Enqueue( new SyncPushRequest(
            userId,
            originDeviceId,
            deletedTaskIds?.ToArray() ?? Array.Empty<long>(),
            deletedHabitIds?.ToArray() ?? Array.Empty<long>() ) );
    }
}
