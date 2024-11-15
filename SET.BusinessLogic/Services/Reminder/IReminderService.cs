namespace BusinessLogic;

public interface IReminderService
{
    Task<TrackingOfUserNotificationRequests> GetNotificationTrackingAsync( long userId );
}