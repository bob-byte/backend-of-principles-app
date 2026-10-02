using AutoMapper;

namespace BusinessLogic;

public class ReminderService : IReminderService
{
    private const int HabitsReportNotificationRequestId = 1;

    private readonly AppDbContext m_dbContext;
    private readonly IMapper m_mapper;

    public ReminderService( AppDbContext dbContext, IMapper mapper )
    {
        m_dbContext = dbContext;
        m_mapper = mapper;
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

    public async Task<UserReminderDto> GetHabitsReportReminderAsync( long userId )
    {
        UserReminder? reminder = await m_dbContext.UserReminders
            .Where( r => r.UserId == userId && r.UserNotificationRequestId == HabitsReportNotificationRequestId )
            .FirstOrDefaultAsync()
            .ConfigureAwait( false );

        return m_mapper.Map<UserReminderDto>( reminder ) ?? new UserReminderDto();
    }

    public async Task<AllRemindersResponse> GetAllRemindersAsync( long userId )
    {
        List<UserReminder> generalReminders = await m_dbContext.UserReminders
            .Where( r => r.UserId == userId )
            .ToListAsync()
            .ConfigureAwait( false );

        List<UserHabitReminder> habitReminders = await m_dbContext.UserHabitReminders
            .Where( r => r.UserHabit.User.Id == userId )
            .Include( r => r.DaysOfWeek )
            .ToListAsync()
            .ConfigureAwait( false );

        return new AllRemindersResponse
        {
            GeneralReminders = m_mapper.Map<List<UserReminderDto>>( generalReminders ) ?? new List<UserReminderDto>(),
            UserHabitReminders = m_mapper.Map<List<UserHabitReminderDto>>( habitReminders ) ?? new List<UserHabitReminderDto>()
        };
    }

    public async Task<ServiceResult<SavedReminderResponse>> SaveHabitsReportReminderAsync( User user, UserReminderDto userReminder )
    {
        #region check parameter
        if (userReminder is null)
        {
            return ServiceError.BadRequest( "HabitsReportReminderIsNullInSaveReminderEndpoint" );
        }
        #endregion

        user.HabitsReportReminder = m_mapper.Map<UserReminder>( userReminder );
        user.HabitsReportReminder!.UserNotificationRequestId = HabitsReportNotificationRequestId;

        await m_dbContext.Users.AddOrUpdateAsync( user ).DefaultConfigureAwait();

        await m_dbContext.SaveChangesAsync().DefaultConfigureAwait();

        return new SavedReminderResponse
        {
            Id = user.HabitsReportReminder!.Id,
            UserNotificationRequestId = user.HabitsReportReminder!.UserNotificationRequestId
        };
    }
}
