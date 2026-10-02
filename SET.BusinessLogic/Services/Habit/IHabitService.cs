namespace BusinessLogic;

/// <summary>
/// Habits with frequency, areas of life and reminders. Writes wake the user's other devices
/// (except <c>originDeviceId</c>) after commit.
/// </summary>
public interface IHabitService
{
    Task<List<UserHabitInProgressShortDto>> GetInProgressAsync( long userId );

    Task<List<ArchivedHabitResponse>> GetArchivedAsync( long userId );

    Task<ServiceResult<EditUserHabitDto>> GetForEditAsync( long habitId );

    /// <summary>Also enables/disables the habit's reminders.</summary>
    Task SetArchiveStatusAsync( HabitArchiveStatus habitArchiveStatus, string? originDeviceId );

    /// <summary>Creates the habit when <c>Id</c> is 0; otherwise updates it. Merges areas of life and reminders.</summary>
    Task<ServiceResult<HabitSavedResponse>> SaveAsync( long userId, EditUserHabitDto habitDto, string? originDeviceId );

    Task<ServiceResult> ResetPrioritiesAsync( long userId, List<UserHabitWithPriority> habits );

    /// <summary>Returns the notification ids the client should cancel.</summary>
    Task<ServiceResult<HabitDeletionResponse>> DeleteAsync( long userId, long habitId, string? originDeviceId );
}
