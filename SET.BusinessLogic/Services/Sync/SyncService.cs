using AutoMapper;

using TaskEntity = SET.Shared.Models.Task;

namespace BusinessLogic;

public class SyncService : ISyncService
{
    /// <summary>Peers older than this must call full bootstrap (tombstones may be gone).</summary>
    private static readonly TimeSpan MaxChangesAge = TimeSpan.FromDays( 14 );

    private readonly AppDbContext m_dbContext;
    private readonly IMapper m_mapper;
    private readonly IReminderService m_reminderService;

    public SyncService( AppDbContext dbContext, IMapper mapper, IReminderService reminderService )
    {
        m_dbContext = dbContext;
        m_mapper = mapper;
        m_reminderService = reminderService;
    }

    public async Task<SyncBootstrapResponse> GetBootstrapAsync( User user )
    {
        DateTime serverTime = DateTime.UtcNow;

        List<UserGoal> goalEntities = await m_dbContext.UserGoals
            .AsNoTracking()
            .Where( g => g.UserId == user.Id )
            .ToListAsync()
            .DefaultConfigureAwait();
        List<UserGoalDto> goals = goalEntities.Select( GoalDtoMapper.ToDto ).ToList();

        List<UserHabit> activeHabits = await m_dbContext.UserHabits
            .Where( u => u.Status == StatusOfHabit.InProgress && u.UserId == user.Id && !u.IsArchived )
            .Include( u => u.Progresses )
            .Include( u => u.Frequency )
            .Include( u => u.Goal )
            .OrderBy( u => u.Priority )
            .AsSplitQuery()
            .ToListAsync()
            .DefaultConfigureAwait();

        List<UserHabitInProgressShortDto> activeDtos = await MapActiveHabitDtosAsync( activeHabits ).DefaultConfigureAwait();

        List<SyncBootstrapArchivedHabitDto> archivedHabits = await m_dbContext.UserHabits
            .Where( u => u.IsArchived && u.UserId == user.Id )
            .OrderByDescending( h => h.Id )
            .Select( h => new SyncBootstrapArchivedHabitDto
            {
                Id = h.Id,
                Name = h.Name,
                IsArchived = true,
                LastModified = h.UpdatedAt ?? h.ArchivingTime ?? h.CreatedAt
            } )
            .ToListAsync()
            .DefaultConfigureAwait();

        UserReminderDto habitsReport = await m_reminderService.GetHabitsReportReminderAsync( user.Id ).DefaultConfigureAwait();
        AllRemindersResponse reminders = await m_reminderService.GetAllRemindersAsync( user.Id ).DefaultConfigureAwait();

        List<TaskEntity> taskEntities = await m_dbContext.Tasks
            .AsNoTracking()
            .Include( t => t.Subtasks )
            .Where( t => t.UserId == user.Id )
            .OrderBy( t => t.Date )
            .ThenBy( t => t.Time )
            .ToListAsync()
            .DefaultConfigureAwait();

        List<AiConversation> conversationEntities = await m_dbContext.AiConversations
            .AsNoTracking()
            .Include( c => c.Messages )
            .Where( c => c.UserId == user.Id )
            .OrderByDescending( c => c.UpdatedAt )
            .ToListAsync()
            .DefaultConfigureAwait();

        return new SyncBootstrapResponse
        {
            ServerTime = serverTime,
            User = new SyncBootstrapUserDto
            {
                Id = user.Id,
                Name = user.Name,
                MainSlogan = user.MainSlogan,
                Mission = user.Mission,
                Email = user.Email,
                Gender = user.Gender,
                LastAppOpen = user.LastAppOpen,
                LastModified = user.UpdatedAt ?? user.CreatedAt
            },
            Goals = goals,
            ActiveHabits = activeDtos,
            ArchivedHabits = archivedHabits,
            HabitsReportReminder = habitsReport,
            GeneralReminders = reminders.GeneralReminders,
            UserHabitReminders = reminders.UserHabitReminders,
            Tasks = taskEntities.Select( TaskDtoMapper.ToDto ).ToList(),
            Conversations = conversationEntities
                .Select( c => AiConversationDtoMapper.ToDto( c ) )
                .ToList(),
        };
    }

    public async Task<SyncChangesResponse> GetChangesAsync( User user, DateTime? since )
    {
        DateTime serverTime = DateTime.UtcNow;
        if (since is null
            || since.Value.Kind == DateTimeKind.Unspecified && since.Value == default
            || serverTime - since.Value.ToUniversalTime() > MaxChangesAge)
        {
            return new SyncChangesResponse
            {
                ServerTime = serverTime,
                RequiresFullBootstrap = true,
            };
        }

        DateTime sinceUtc = since.Value.Kind == DateTimeKind.Utc
            ? since.Value
            : since.Value.ToUniversalTime();

        SyncBootstrapUserDto? userDto = null;
        DateTime userStamp = user.UpdatedAt ?? user.CreatedAt;
        if (userStamp > sinceUtc)
        {
            userDto = new SyncBootstrapUserDto
            {
                Id = user.Id,
                Name = user.Name,
                MainSlogan = user.MainSlogan,
                Mission = user.Mission,
                Email = user.Email,
                Gender = user.Gender,
                LastAppOpen = user.LastAppOpen,
                LastModified = userStamp,
            };
        }

        List<UserGoal> changedGoals = await m_dbContext.UserGoals
            .AsNoTracking()
            .Where( g => g.UserId == user.Id
                         && ( ( g.UpdatedAt ?? g.ArchivingTime ?? g.CreatedAt ) > sinceUtc ) )
            .ToListAsync()
            .DefaultConfigureAwait();
        List<UserGoalDto> goals = changedGoals.Select( GoalDtoMapper.ToDto ).ToList();

        List<UserHabit> changedActive = await m_dbContext.UserHabits
            .Where( u => u.Status == StatusOfHabit.InProgress
                         && u.UserId == user.Id
                         && !u.IsArchived
                         && ( ( u.UpdatedAt ?? u.CreatedAt ) > sinceUtc ) )
            .Include( u => u.Progresses )
            .Include( u => u.Frequency )
            .Include( u => u.Goal )
            .OrderBy( u => u.Priority )
            .AsSplitQuery()
            .ToListAsync()
            .DefaultConfigureAwait();

        List<UserHabitInProgressShortDto> activeDtos = await MapActiveHabitDtosAsync( changedActive ).DefaultConfigureAwait();

        List<SyncBootstrapArchivedHabitDto> archivedHabits = await m_dbContext.UserHabits
            .Where( u => u.IsArchived
                         && u.UserId == user.Id
                         && ( ( u.UpdatedAt ?? u.ArchivingTime ?? u.CreatedAt ) > sinceUtc ) )
            .OrderByDescending( h => h.Id )
            .Select( h => new SyncBootstrapArchivedHabitDto
            {
                Id = h.Id,
                Name = h.Name,
                IsArchived = true,
                LastModified = h.UpdatedAt ?? h.ArchivingTime ?? h.CreatedAt
            } )
            .ToListAsync()
            .DefaultConfigureAwait();

        List<TaskEntity> changedTasks = await m_dbContext.Tasks
            .AsNoTracking()
            .Include( t => t.Subtasks )
            .Where( t => t.UserId == user.Id && t.UpdatedAt > sinceUtc )
            .OrderBy( t => t.Date )
            .ThenBy( t => t.Time )
            .ToListAsync()
            .DefaultConfigureAwait();

        List<AiConversation> changedConversations = await m_dbContext.AiConversations
            .AsNoTracking()
            .Include( c => c.Messages )
            .Where( c => c.UserId == user.Id && c.UpdatedAt > sinceUtc )
            .OrderByDescending( c => c.UpdatedAt )
            .ToListAsync()
            .DefaultConfigureAwait();

        // Reminders are small; always send the current lists so peers stay aligned
        // without reminder timestamps / tombstones.
        UserReminderDto habitsReport = await m_reminderService.GetHabitsReportReminderAsync( user.Id ).DefaultConfigureAwait();
        AllRemindersResponse reminders = await m_reminderService.GetAllRemindersAsync( user.Id ).DefaultConfigureAwait();

        List<SyncDeletion> deletions = await m_dbContext.SyncDeletions
            .AsNoTracking()
            .Where( d => d.UserId == user.Id && d.DeletedAt > sinceUtc )
            .ToListAsync()
            .DefaultConfigureAwait();

        return new SyncChangesResponse
        {
            ServerTime = serverTime,
            RequiresFullBootstrap = false,
            User = userDto,
            Goals = goals,
            ActiveHabits = activeDtos,
            ArchivedHabits = archivedHabits,
            HabitsReportReminder = habitsReport,
            GeneralReminders = reminders.GeneralReminders,
            UserHabitReminders = reminders.UserHabitReminders,
            Tasks = changedTasks.Select( TaskDtoMapper.ToDto ).ToList(),
            Conversations = changedConversations
                .Select( c => AiConversationDtoMapper.ToDto( c ) )
                .ToList(),
            DeletedGoalIds = DeletedIds( deletions, SyncEntityTypes.Goal ),
            DeletedHabitIds = DeletedIds( deletions, SyncEntityTypes.Habit ),
            DeletedTaskIds = DeletedIds( deletions, SyncEntityTypes.Task ),
            DeletedConversationIds = DeletedIds( deletions, SyncEntityTypes.Conversation ),
        };
    }

    private static List<long> DeletedIds( List<SyncDeletion> deletions, string entityType )
    {
        return deletions
            .Where( d => d.EntityType == entityType )
            .Select( d => d.EntityId )
            .Distinct()
            .ToList();
    }

    private async Task<List<UserHabitInProgressShortDto>> MapActiveHabitDtosAsync( List<UserHabit> habits )
    {
        List<UserHabitInProgressShortDto> dtos = new();
        foreach (UserHabit habit in habits)
        {
            dtos.Add( await MapActiveHabitDtoAsync( habit ).DefaultConfigureAwait() );
        }

        return dtos;
    }

    private async Task<UserHabitInProgressShortDto> MapActiveHabitDtoAsync( UserHabit habit )
    {
        UserHabitInProgressShortDto habitDto = m_mapper.Map<UserHabitInProgressShortDto>( habit );
        habitDto.LastModified = habit.UpdatedAt ?? habit.CreatedAt;

        UserHabitReminder? reminder = await m_dbContext.UserHabitReminders
            .Where( r => r.UserHabitId == habit.Id )
            .Include( r => r.DaysOfWeek )
            .FirstOrDefaultAsync()
            .DefaultConfigureAwait();

        if (reminder is not null)
        {
            habitDto.Reminders = new List<UserHabitReminderDto>
            {
                m_mapper.Map<UserHabitReminderDto>( reminder )
            };
        }

        return habitDto;
    }
}
