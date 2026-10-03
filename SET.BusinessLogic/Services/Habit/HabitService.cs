using AutoMapper;

using Microsoft.EntityFrameworkCore.Storage;

namespace BusinessLogic;

public class HabitService : IHabitService
{
    private readonly AppDbContext m_dbContext;
    private readonly IMapper m_mapper;
    private readonly IReminderService m_reminderService;
    private readonly ISyncPushService m_syncPushService;

    public HabitService(
        AppDbContext dbContext,
        IMapper mapper,
        IReminderService reminderService,
        ISyncPushService syncPushService )
    {
        m_dbContext = dbContext;
        m_mapper = mapper;
        m_reminderService = reminderService;
        m_syncPushService = syncPushService;
    }

    public async Task<List<UserHabitInProgressShortDto>> GetInProgressAsync( long userId )
    {
        List<UserHabit> listOfHabits = await m_dbContext.UserHabits.
            Where( u => u.Status == StatusOfHabit.InProgress && u.UserId == userId && !u.IsArchived ).
            Include( u => u.Progresses ).
            Include( u => u.Frequency ).
            Include( u => u.Goal ).
            OrderBy( u => u.Priority ).
            AsSplitQuery().
            ToListAsync().
            DefaultConfigureAwait();

        List<UserHabitInProgressShortDto> resultData = new();

        foreach (UserHabit habit in listOfHabits)
        {
            UserHabitInProgressShortDto habitDto = m_mapper.Map<UserHabitInProgressShortDto>( habit );
            habitDto.AreasOfLife = await LoadAreasOfLifeAsync( habit.Id ).DefaultConfigureAwait();

            UserHabitReminderDto? reminder = await LoadReminderAsync( habit.Id ).DefaultConfigureAwait();
            if (reminder is not null)
            {
                habitDto.Reminders = new List<UserHabitReminderDto> { reminder };
            }

            resultData.Add( habitDto );
        }

        return resultData;
    }

    public Task<List<ArchivedHabitResponse>> GetArchivedAsync( long userId )
    {
        return m_dbContext.UserHabits
            .Where( u => u.IsArchived && u.UserId == userId )
            .Select( h => new ArchivedHabitResponse { Id = h.Id, Name = h.Name } )
            .OrderByDescending( h => h.Id )
            .ToListAsync();
    }

    public async Task<ServiceResult<EditUserHabitDto>> GetForEditAsync( long userId, long habitId )
    {
        UserHabit? habit = await m_dbContext.UserHabits.
            Where( u => u.Id == habitId && u.UserId == userId ).
            Include( u => u.Frequency ).
            Include( u => u.Goal ).
            Include( u => u.Progresses ).
            FirstOrDefaultAsync().
            DefaultConfigureAwait();

        if (habit == null)
        {
            return ServiceError.BadRequest( $"{nameof( UserHabit )} is not found" );
        }

        EditUserHabitDto resultData = m_mapper.Map<EditUserHabitDto>( habit );
        resultData.AreasOfLife = await LoadAreasOfLifeAsync( habitId ).DefaultConfigureAwait();

        UserHabitReminderDto? reminder = await LoadReminderAsync( habitId ).DefaultConfigureAwait();
        if (reminder is not null)
        {
            resultData.Reminders = new List<UserHabitReminderDto> { reminder };
        }

        return resultData;
    }

    public async Task<ServiceResult> SetArchiveStatusAsync( long userId, HabitArchiveStatus habitArchiveStatus, string? originDeviceId )
    {
        if (habitArchiveStatus is null)
        {
            return ServiceError.BadRequest( "HabitArchiveStatusIsNull" );
        }

        UserHabit? habit = await m_dbContext.UserHabits
            .Where( u => u.Id == habitArchiveStatus.HabitId && u.UserId == userId )
            .FirstOrDefaultAsync()
            .DefaultConfigureAwait();

        if (habit is null)
        {
            return ServiceError.NotFound( $"HabitIsNotFoundWithId {habitArchiveStatus.HabitId}" );
        }

        habit.IsArchived = habitArchiveStatus.IsArchived;

        List<UserHabitReminder> habitReminders = await m_dbContext.
            UserHabitReminders.
            Where( r => r.UserHabitId == habitArchiveStatus.HabitId ).
            ToListAsync().
            DefaultConfigureAwait();
        foreach (UserHabitReminder reminder in habitReminders)
        {
            reminder.IsEnabled = !habit.IsArchived;
        }

        habit.UpdatedAt = DateTime.UtcNow;
        habit.ArchivingTime = habit.IsArchived ? habit.UpdatedAt : null;

        await m_dbContext.SaveChangesAsync().DefaultConfigureAwait();
        m_syncPushService.NotifyOtherDevices( habit.UserId, originDeviceId );
        return ServiceResult.Success;
    }

    public async Task<ServiceResult<HabitSavedResponse>> SaveAsync( long userId, EditUserHabitDto habitDto, string? originDeviceId )
    {
        #region Check habitDto param
        if (habitDto is null)
        {
            return ServiceError.BadRequest( "HabitIsNull" );
        }

        if (habitDto.Frequency == null)
        {
            return ServiceError.BadRequest( "FrequencyIsNull" );
        }
        #endregion

        await using IDbContextTransaction transaction = await m_dbContext.Database.BeginTransactionAsync().DefaultConfigureAwait();

        try
        {
            List<UserHabit> userHabitList = await m_dbContext.UserHabits.Where( h => h.UserId == userId )
                .ToListAsync().DefaultConfigureAwait();

            foreach (UserHabit userHabit in userHabitList.Where( h => h.Id != habitDto.Id ))
            {
                int? updatedPriority = habitDto.PrioritizedHabits?.Find( h => h.Id == userHabit.Id )?.Priority;
                if (updatedPriority != null)
                {
                    userHabit.Priority = (int)updatedPriority;
                }
            }

            UserHabit? habit = habitDto.Id == 0
                ? null
                : userHabitList.Find( h => h.Id == habitDto.Id );
            if (habit is null && habitDto.Id != 0)
            {
                return ServiceError.NotFound( $"HabitIsNotFoundWithId {habitDto.Id}" );
            }

            bool isNewHabit = habit == null;

            if (habit is null)
            {
                habit = m_mapper.Map<UserHabit>( habitDto );
                habit.UserId = userId;
                habit.FrequencyId = 0;
                habit.Frequency.Id = 0;
                habit.Goal = null;
                habit.GoalId = habitDto.Goal?.Id > 0 ? habitDto.Goal.Id : null;
                habit.ColorName = SanitizeHabitColorName( habitDto.ColorName );
                habit.IsArchived = habitDto.IsArchived;
                habit.CreatedAt = DateTime.UtcNow;

                if (habit.IsArchived)
                {
                    habit.ArchivingTime = habit.CreatedAt;
                }
            }
            else
            {
                habit.Name = habitDto.Name;
                habit.Frequency = m_mapper.Map<Frequency>( habitDto.Frequency )!;
                habit.Frequency.Id = habit.FrequencyId;
                habit.ColorName = SanitizeHabitColorName( habitDto.ColorName );
                habit.Description = habitDto.Description;
                habit.Question = habitDto.Question;
                habit.Complexity = habitDto.Complexity;
                habit.Type = habitDto.Type;
                habit.Priority = habitDto.Priority;
                habit.Goal = null;
                habit.GoalId = habitDto.Goal?.Id > 0 ? habitDto.Goal.Id : null;
                habit.IsArchived = habitDto.IsArchived;
                habit.UpdatedAt = DateTime.UtcNow;
                habit.ArchivingTime = habit.IsArchived ? habit.UpdatedAt : null;
                habit.Kind = habitDto.Kind;
                habit.Unit = habitDto.Unit;
                habit.TargetPerOneTime = habitDto.TargetPerOneTime;
                habit.TargetType = habitDto.TargetType;
                habit.MinRate = habitDto.MinRate;
                habit.MaxRate = habitDto.MaxRate;
                habit.ProgressMarkVariaty = habitDto.ProgressMarkVariaty;
            }

            habit.GoalId = await ResolveUserGoalIdAsync( userId, habit.GoalId )
                .DefaultConfigureAwait();
            if (habit.Frequency == null)
            {
                return ServiceError.BadRequest( "FrequencyIsNull" );
            }

            if (isNewHabit)
            {
                if (habitDto.AreasOfLife?.Any() == true)
                {
                    foreach (UserAreaOfLifeDto area in habitDto.AreasOfLife)
                    {
                        await m_dbContext.UserAreasOfLifeUserHabits.AddAsync( new UserAreaOfLifeUserHabit
                        {
                            Habit = habit, AreaOfLifeId = area.Id
                        } );
                    }
                }
            }
            else
            {
                UserAreaOfLifeDto[] sourceAreas = habitDto.AreasOfLife;

                UserAreaOfLifeUserHabit[] targetAreasAndHabits = await m_dbContext.UserAreasOfLifeUserHabits
                    .Where( u => u.HabitId == habit.Id ).ToArrayAsync().DefaultConfigureAwait();

                await m_dbContext.UserAreasOfLifeUserHabits.MergeAsync(
                    targetAreasAndHabits,
                    sourceAreas,
                    ( areasToInsert ) =>
                    {
                        return areasToInsert.Select(
                            area => new UserAreaOfLifeUserHabit() { AreaOfLifeId = area.Id, HabitId = habit.Id }
                        );
                    },
                    t => t.AreaOfLifeId,
                    s => s.Id
                );
            }

            if (isNewHabit)
            {
                await m_dbContext.UserHabits.AddOrUpdateAsync( habit ).DefaultConfigureAwait();
            }
            else
            {
                userHabitList.Remove( habit );
            }

            await m_dbContext.Frequencies.AddOrUpdateAsync( habit.Frequency ).DefaultConfigureAwait();
            m_dbContext.UserHabits.UpdateRange( userHabitList );
            await m_dbContext.SaveChangesAsync().DefaultConfigureAwait();

            await SaveRemindersAsync( userId, habit, habitDto, isNewHabit ).DefaultConfigureAwait();

            await m_dbContext.SaveChangesAsync().DefaultConfigureAwait();

            List<ReminderIds> reminderDetails = await m_dbContext.UserHabitReminders
                .Where( r => r.UserHabitId == habit.Id )
                .Select( r => new ReminderIds
                {
                    Id = r.Id,
                    DaysOfWeek = r.DaysOfWeek.Select( d => new WeekDayIds
                    {
                        Id = d.Id, Type = d.Type, NotificationRequestId = d.UserNotificationRequestId
                    } ).ToList()
                } )
                .ToListAsync().DefaultConfigureAwait();

            await transaction.CommitAsync().DefaultConfigureAwait();
            m_syncPushService.NotifyOtherDevices( userId, originDeviceId );

            return new HabitSavedResponse
            {
                Id = habit.Id,
                FrequencyId = habit.FrequencyId,
                ReminderIds = reminderDetails
            };
        }
        catch
        {
            await transaction.RollbackAsync().DefaultConfigureAwait();
            throw;
        }
    }

    public async Task<ServiceResult> ResetPrioritiesAsync( long userId, List<UserHabitWithPriority> habits )
    {
        #region Check param
        if (habits == null || habits.Count < 2)
        {
            return ServiceError.BadRequest( "Habits with priorities are less than 2" );
        }
        #endregion

        List<UserHabit> userHabitList = await m_dbContext.
            UserHabits.
            Where( h => h.UserId == userId ).
            ToListAsync().
            DefaultConfigureAwait();

        foreach (UserHabit userHabit in userHabitList)
        {
            int updatedPriority = habits.Find( h => h.Id == userHabit.Id )!.Priority;
            userHabit.Priority = updatedPriority;
        }

        await m_dbContext.SaveChangesAsync().DefaultConfigureAwait();
        return ServiceResult.Success;
    }

    public async Task<ServiceResult<HabitDeletionResponse>> DeleteAsync( long userId, long habitId, string? originDeviceId )
    {
        #region Check parameter
        if (habitId == 0)
        {
            return ServiceError.BadRequest( "HabitIdIsZero" );
        }
        #endregion

        UserHabit? habit = await m_dbContext.UserHabits
            .Where( h => h.Id == habitId && h.UserId == userId )
            .FirstOrDefaultAsync()
            .DefaultConfigureAwait();

        if (habit is null)
        {
            return ServiceError.NotFound( $"HabitIsNotFoundWithId {habitId}" );
        }

        long[] reminderIds = await m_dbContext.UserHabitReminders.Where( r => r.UserHabitId == habitId ).Select( r => r.Id ).ToArrayAsync().DefaultConfigureAwait();
        List<HabitDeletionResponse.NotificationRequest> notificationRequests = new();
        foreach (long idOfReminder in reminderIds)
        {
            List<WeekDay> weekDaysOfReminder = await m_dbContext.WeekDays.Where( w => w.UserHabitReminderId == idOfReminder ).ToListAsync().DefaultConfigureAwait();
            notificationRequests.AddRange( weekDaysOfReminder.Select( w => new HabitDeletionResponse.NotificationRequest( w.UserNotificationRequestId ) ) );
        }

        HabitDeletionResponse response = new();
        response.DeletedNotifications = notificationRequests;

        await using IDbContextTransaction transaction = await m_dbContext.Database.BeginTransactionAsync().DefaultConfigureAwait();

        try
        {
            m_dbContext.SyncDeletions.Add( new SyncDeletion
            {
                UserId = userId,
                EntityType = SyncEntityTypes.Habit,
                EntityId = habitId,
                DeletedAt = DateTime.UtcNow,
            } );
            await m_dbContext.SaveChangesAsync().DefaultConfigureAwait();

            await m_dbContext.UserAreasOfLifeUserHabits.Where( p => p.HabitId == habitId ).ExecuteDeleteAsync()
                .DefaultConfigureAwait();
            await m_dbContext.UserHabits.Where( u => u.Id == habitId ).ExecuteDeleteAsync()
                .DefaultConfigureAwait();
            await m_dbContext.Frequencies.Where( f => f.Id == habit.FrequencyId ).ExecuteDeleteAsync()
                .DefaultConfigureAwait();

            await transaction.CommitAsync().DefaultConfigureAwait();
            m_syncPushService.NotifyOtherDevices( userId, originDeviceId, deletedHabitIds: new[] { habitId } );
        }
        catch
        {
            await transaction.RollbackAsync().DefaultConfigureAwait();
            throw;
        }

        return response;
    }

    private async Task SaveRemindersAsync( long userId, UserHabit habit, EditUserHabitDto habitDto, bool isNewHabit )
    {
        if (habitDto.Reminders?.Any() == true)
        {
            TrackingOfUserNotificationRequests notificationRequest =
                await m_reminderService.GetNotificationTrackingAsync( userId ).DefaultConfigureAwait();

            foreach (WeekDayDto weekDay in habitDto.Reminders.SelectMany( r => r.DaysOfWeek ))
            {
                if (weekDay.UserNotificationRequestId == 0)
                {
                    weekDay.UserNotificationRequestId = ++notificationRequest.MaxNotificationRequestId;
                }
            }
        }

        if ((isNewHabit && habitDto.Reminders is not null) ||
            habitDto.Reminders?.All( r => r.Id == 0 ) == true)
        {
            foreach (UserHabitReminderDto reminder in habitDto.Reminders)
            {
                await m_dbContext.UserHabitReminders.AddAsync( new UserHabitReminder
                {
                    UserHabitId = habit.Id,
                    Title = reminder.Title,
                    Description = reminder.Description,
                    Time = reminder.Time,
                    IsEnabled = reminder.IsEnabled,
                    DaysOfWeek = reminder.DaysOfWeek.Select( d => new WeekDay
                    {
                        Type = d.Type, UserNotificationRequestId = d.UserNotificationRequestId
                    } ).ToList()
                } ).DefaultConfigureAwait();
            }

            return;
        }

        UserHabitReminderDto[] sourceReminders =
            habitDto.Reminders?.ToArray() ?? Array.Empty<UserHabitReminderDto>();

        UserHabitReminder[] targetReminders = await m_dbContext.UserHabitReminders
            .Where( u => u.UserHabitId == habit.Id )
            .Include( r => r.DaysOfWeek )
            .ToArrayAsync()
            .ConfigureAwait( false );

        await m_dbContext.UserHabitReminders.MergeAsync(
            targetReminders,
            sourceReminders,
            remindersToInsert =>
            {
                return remindersToInsert.Select( reminder => new UserHabitReminder
                {
                    Title = reminder.Title,
                    Description = reminder.Description,
                    Time = reminder.Time,
                    IsEnabled = reminder.IsEnabled,
                    UserHabitId = habit.Id,
                    DaysOfWeek = reminder.DaysOfWeek.Select( d => new WeekDay
                    {
                        Type = d.Type, UserNotificationRequestId = d.UserNotificationRequestId
                    } ).ToList()
                } );
            },
            t => t.Id,
            s => s.Id,
            ( existingReminder, updatedReminder ) =>
            {
                existingReminder.Title = updatedReminder.Title;
                existingReminder.Description = updatedReminder.Description;
                existingReminder.Time = updatedReminder.Time;
                existingReminder.IsEnabled = updatedReminder.IsEnabled;
            }
        ).DefaultConfigureAwait();

        foreach (UserHabitReminder targetReminder in targetReminders)
        {
            UserHabitReminderDto? sourceReminder =
                sourceReminders.FirstOrDefault( r => r.Id == targetReminder.Id );
            if (sourceReminder != null)
            {
                await m_dbContext.WeekDays.MergeAsync(
                    targetReminder.DaysOfWeek.ToArray(),
                    sourceReminder.DaysOfWeek,
                    daysToInsert => daysToInsert.Select( d => new WeekDay
                    {
                        Type = d.Type,
                        UserNotificationRequestId = d.UserNotificationRequestId,
                        UserHabitReminderId = targetReminder.Id
                    } ),
                    t => t.Id,
                    s => s.Id,
                    ( existingDay, updatedDay ) =>
                    {
                        existingDay.Type = updatedDay.Type;
                        existingDay.UserNotificationRequestId = updatedDay.UserNotificationRequestId;
                    }
                ).DefaultConfigureAwait();
            }
        }

        await m_dbContext.SaveChangesAsync().DefaultConfigureAwait();
    }

    private async Task<UserAreaOfLifeDto[]> LoadAreasOfLifeAsync( long habitId )
    {
        List<UserAreaOfLife> areasOfLife = await m_dbContext.
            UserAreasOfLife.
            Include( u => u.Habits ).
            Where( u => u.Habits.Any( up => up.HabitId == habitId ) ).
            ToListAsync().
            DefaultConfigureAwait();

        return m_mapper.Map<UserAreaOfLifeDto[]>( areasOfLife );
    }

    private async Task<UserHabitReminderDto?> LoadReminderAsync( long habitId )
    {
        UserHabitReminder? reminder = await m_dbContext.UserHabitReminders
            .Where( r => r.UserHabitId == habitId )
            .Include( r => r.DaysOfWeek )
            .FirstOrDefaultAsync()
            .DefaultConfigureAwait();

        return reminder is null ? null : m_mapper.Map<UserHabitReminderDto>( reminder );
    }

    private static string SanitizeHabitColorName( string? colorName )
    {
        const string fallback = "#1C1C1C";
        if (string.IsNullOrWhiteSpace( colorName ))
        {
            return fallback;
        }

        return colorName.Length <= 10 ? colorName : colorName[..10];
    }

    private async Task<long?> ResolveUserGoalIdAsync( long userId, long? goalId )
    {
        if (goalId is not > 0)
        {
            return null;
        }

        bool exists = await m_dbContext.UserGoals
            .AnyAsync( g => g.Id == goalId && g.UserId == userId )
            .DefaultConfigureAwait();
        return exists ? goalId : null;
    }
}
