namespace BusinessLogic;

/// <summary>
/// A change that other installs of the user should pull via <c>/sync/changes</c>.
/// </summary>
/// <param name="OriginDeviceId">The install that made the change (from <c>X-Device-Id</c>); it is not woken.</param>
public sealed record SyncPushRequest(
    long UserId,
    string? OriginDeviceId,
    IReadOnlyCollection<long> DeletedTaskIds,
    IReadOnlyCollection<long> DeletedHabitIds );

/// <summary>
/// Wakes the user's other devices with a silent (data-only) push so they sync and fix
/// local reminders for tasks/habits changed or deleted elsewhere.
/// </summary>
public interface ISyncPushService
{
    /// <summary>Queues a push; never blocks or throws on the request path.</summary>
    void Enqueue( SyncPushRequest request );
}
