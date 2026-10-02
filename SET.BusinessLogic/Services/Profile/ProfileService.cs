using AutoMapper;

namespace BusinessLogic;

public class ProfileService : IProfileService
{
    private readonly AppDbContext m_dbContext;
    private readonly IMapper m_mapper;

    public ProfileService( AppDbContext dbContext, IMapper mapper )
    {
        m_dbContext = dbContext;
        m_mapper = mapper;
    }

    public Models.Profile GetProfile( User user )
    {
        Models.Profile data = m_mapper.Map<Models.Profile>( user );
        data.Id = user.Id;
        data.LastModified = user.UpdatedAt ?? user.CreatedAt;
        return data;
    }

    public async Task<ServiceResult> SaveNameAsync( User user, string userName )
    {
        if (string.IsNullOrWhiteSpace( userName ))
        {
            return ServiceError.BadRequest( "UserNameIsNullOrWhiteSpace" );
        }

        user.Name = userName;
        await SaveUserAsync( user ).DefaultConfigureAwait();
        return ServiceResult.Success;
    }

    public Task SaveMainSloganAsync( User user, string mainSlogan )
    {
        user.MainSlogan = mainSlogan;
        return SaveUserAsync( user );
    }

    public async Task<ServiceResult> SaveGenderAsync( User user, Gender gender )
    {
        if (!Enum.IsDefined( typeof( Gender ), gender ))
        {
            return ServiceError.BadRequest( "GenderIsInvalid" );
        }

        user.Gender = gender;
        await SaveUserAsync( user ).DefaultConfigureAwait();
        return ServiceResult.Success;
    }

    public Task SaveHasSeenRoadGuideAsync( User user, bool hasSeenRoadGuide )
    {
        user.HasSeenRoadGuide = hasSeenRoadGuide;
        return SaveUserAsync( user );
    }

    public async Task SaveMissionAsync( User user, string mission )
    {
        string? oldMission = (string?)user.Mission?.Clone();
        user.Mission = mission;

        if (oldMission is not null)
        {
            List<UserHabitReminder> remindersToUpdate = await m_dbContext.UserHabitReminders.
                Include( r => r.UserHabit ).
                Where( r => r.Title == oldMission && r.UserHabit.UserId == user.Id ).
                ToListAsync().
                DefaultConfigureAwait();
            List<UserReminder> userReminders =
                await m_dbContext.UserReminders.Where( r => r.UserId == user.Id && (r.Title == oldMission || r.Description == oldMission) ).ToListAsync().DefaultConfigureAwait();

            if (remindersToUpdate?.Count > 0)
            {
                foreach (UserHabitReminder reminder in remindersToUpdate)
                {
                    reminder.Title = mission;
                }

                m_dbContext.UserHabitReminders.UpdateRange( remindersToUpdate );
            }

            if (userReminders?.Count > 0)
            {
                foreach (UserReminder reminder in userReminders)
                {
                    if (reminder.Title == oldMission)
                    {
                        reminder.Title = mission;
                    }

                    if (reminder.Description == oldMission)
                    {
                        reminder.Description = mission;
                    }
                }

                m_dbContext.UserReminders.UpdateRange( userReminders );
            }
        }

        await SaveUserAsync( user ).DefaultConfigureAwait();
    }

    private async Task SaveUserAsync( User user )
    {
        user.UpdatedAt = DateTime.UtcNow;
        m_dbContext.Users.Update( user );

        await m_dbContext.SaveChangesAsync().DefaultConfigureAwait();
    }
}
