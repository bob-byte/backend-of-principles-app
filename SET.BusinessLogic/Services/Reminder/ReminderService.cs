namespace BusinessLogic;

public class ReminderService : IReminderService
{
    private readonly AppDbContext m_dbContext;

    public ReminderService( AppDbContext dbContext )
    {
        m_dbContext = dbContext;
    }

    public async Task<TrackingOfUserNotificationRequests> GetNotificationTrackingAsync( long userId )
    {
        TrackingOfUserNotificationRequests? trackingOfNotifications = await m_dbContext.TrackingOfUserNotificationRequests
            .FirstOrDefaultAsync( u => u.UserId == userId )
            .ConfigureAwait( false );

        if (trackingOfNotifications is null)
        {
            trackingOfNotifications = new TrackingOfUserNotificationRequests
            {
                UserId = userId,
                MaxNotificationRequestId = 99  //99 because it will be incremented
            };

            m_dbContext.TrackingOfUserNotificationRequests.Add( trackingOfNotifications );
            await m_dbContext.SaveChangesAsync().ConfigureAwait( false );
        }

        return trackingOfNotifications;
    }
}