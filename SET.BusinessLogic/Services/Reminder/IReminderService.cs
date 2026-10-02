namespace BusinessLogic;

public interface IReminderService
{
    Task<TrackingOfUserNotificationRequests> GetNotificationTrackingAsync( long userId );

    /// <summary>The daily habits-report reminder, or an empty DTO when the user has none.</summary>
    Task<UserReminderDto> GetHabitsReportReminderAsync( long userId );

    Task<AllRemindersResponse> GetAllRemindersAsync( long userId );

    Task<ServiceResult<SavedReminderResponse>> SaveHabitsReportReminderAsync( User user, UserReminderDto userReminder );
}
