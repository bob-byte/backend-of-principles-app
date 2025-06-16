
using Microsoft.EntityFrameworkCore.Storage;

namespace SET.WebAPI.Controllers;

[Route( template: "api/habits")]
[ApiController]
[Authorize( AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme )]
public class HabitController : BaseController
{
    private readonly IReminderService m_reminderService;
    
    public HabitController( IServiceProvider serviceProvider, IReminderService reminderService )
        : base( serviceProvider )
    {
        m_reminderService = reminderService;
    }

    [HttpGet( template: "inprogress" )]
    public Task<IActionResult> InProgressIndex()
    {
        return TryCatchAsync( async ( user ) =>
        {
            List<UserHabit> listOfHabits = await DbContext.UserHabits.
                Where( u => u.Status == StatusOfHabit.InProgress && u.UserId == user.Id && !u.IsArchived ).
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
                List<UserAreaOfLife> areasOfLife = await DbContext.
                    UserAreasOfLife.
                    Include( u => u.Habits ).
                    Where( u => u.Habits.Any( up => up.HabitId == habit.Id )).
                    ToListAsync().
                    DefaultConfigureAwait();

                UserHabitInProgressShortDto habitDto = Mapper.Map<UserHabitInProgressShortDto>( habit );
                habitDto.AreasOfLife = Mapper.Map<UserAreaOfLifeDto[]>( areasOfLife );

                UserHabitReminder? reminder = await DbContext.UserHabitReminders
                    .Where( r => r.UserHabitId == habit.Id )
                    .Include( r => r.DaysOfWeek )
                    .FirstOrDefaultAsync()
                    .DefaultConfigureAwait();

                if (reminder is not null)
                {
                    UserHabitReminderDto dtoReminder = Mapper.Map<UserHabitReminderDto>( reminder );

                    habitDto.Reminders = new List<UserHabitReminderDto>
                    {
                        dtoReminder
                    };
                }
                
                resultData.Add( habitDto );
            }
            
            OkObjectResult result = Ok( resultData );
            return result;
        } );
    }

    [HttpGet( template: "archive" )]
    public Task<IActionResult> GetArchiveHabits()
    {
        return TryCatchAsync( async ( user ) =>
        {
            OkObjectResult result =  Ok( await DbContext.UserHabits
                .Where( u => u.IsArchived && u.UserId == user.Id )
                .Select( h => new { h.Id, h.Name } )
                .OrderByDescending( h => h.Id )
                .ToListAsync()
                .DefaultConfigureAwait() );
            return result;
        } );
    }
    
    [HttpGet(template: "{habitId}")]
    public Task<IActionResult> Load( long habitId )
    {
        return TryCatchAsync( async () =>
        {
            UserHabit habit = await DbContext.UserHabits.
                Where( u => u.Id == habitId ).
                Include( u => u.Frequency ).
                Include( u => u.Goal ).
                Include( u => u.Progresses ).
                FirstOrDefaultAsync().
                DefaultConfigureAwait();

            IActionResult result;
            if(habit == null)
            {
                result = BadRequest( $"{nameof(UserHabit)} is not found" );
            }
            else
            {
                List<UserAreaOfLife> areasOfLife = await DbContext.
                    UserAreasOfLife.
                    Include( u => u.Habits ).
                    Where( u => u.Habits.Any( up => up.HabitId == habitId )).
                    ToListAsync().
                    DefaultConfigureAwait();

                EditUserHabitDto resultData = Mapper.Map<EditUserHabitDto>( habit );
                resultData.AreasOfLife = Mapper.Map<UserAreaOfLifeDto[]>( areasOfLife );

                UserHabitReminder? reminder = await DbContext.UserHabitReminders
                    .Where( r => r.UserHabitId == habitId )
                    .Include( r => r.DaysOfWeek )
                    .FirstOrDefaultAsync()
                    .DefaultConfigureAwait();

                if (reminder is not null)
                {
                    UserHabitReminderDto dtoReminder = Mapper.Map<UserHabitReminderDto>( reminder );

                    resultData.Reminders = new List<UserHabitReminderDto>
                    {
                        dtoReminder
                    };
                }

                result = Ok( resultData );
            }

            return result;
        } );
    }

    [HttpPost( template: "archivestatus" )]
    public async Task<IActionResult> SetHabitArchiveStatus( [FromBody] HabitArchiveStatus habitArchiveStatus )
    {
        return await TryCatchAsync( async () =>
        {
            UserHabit habit = await DbContext.UserHabits
                .Where( u => u.Id == habitArchiveStatus.HabitId )
                .FirstOrDefaultAsync()
                .DefaultConfigureAwait();

            habit.IsArchived = habitArchiveStatus.IsArchived;
            habit.UpdatedAt = DateTime.UtcNow;
            habit.ArchivingTime = habitArchiveStatus.IsArchived ? habit.UpdatedAt : null;

            await DbContext.SaveChangesAsync().DefaultConfigureAwait();

            return Ok();
        } );
    }

    [HttpPost( template: "{habitId}" )]
    public Task<IActionResult> Update( [FromBody] EditUserHabitDto habitDto )
    {
        return TryCatchAsync( async (User user) =>
        {
            #region Check habitDto param
            bool isCorrectArg = habitDto != null;

            if(!isCorrectArg)
            {
                return BadRequest( "HabitIsNull" );
            }

            if (habitDto.Frequency == null)
            {
                return BadRequest( "FrequencyIsNull" );
            }
            #endregion
            
            await using IDbContextTransaction transaction = await DbContext.Database.BeginTransactionAsync().DefaultConfigureAwait();

            try
            {
                List<UserHabit> userHabitList = await DbContext.UserHabits.Where( h => h.UserId == user.Id )
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
                bool isNewHabit = habit == null;

                if (isNewHabit)
                {
                    habit = Mapper.Map<UserHabit>( habitDto );
                    habit.UserId = user.Id;
                    habit.Goal = null;
                    habit.GoalId = habitDto.Goal?.Id > 0 ? habitDto.Goal.Id : null;
                    habit.IsArchived = habitDto.IsArchived;
                    habit.CreatedAt = DateTime.UtcNow;
                    habit.ArchivingTime = habit.CreatedAt;
                }
                else
                {
                    habit.Name = habitDto.Name;
                    habit.FrequencyId = habitDto.Frequency.Id;
                    habit.Frequency = Mapper.Map<Frequency>( habitDto.Frequency );
                    habit.ColorName = habitDto.ColorName;
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
                }

                if (isNewHabit)
                {
                    if (habitDto.AreasOfLife?.Any() == true)
                    {
                        foreach (UserAreaOfLifeDto area in habitDto.AreasOfLife)
                        {
                            await DbContext.UserAreasOfLifeUserHabits.AddAsync( new UserAreaOfLifeUserHabit
                            {
                                Habit = habit, AreaOfLifeId = area.Id
                            } );
                        }
                    }
                }
                else
                {
                    UserAreaOfLifeDto[] sourceAreas = habitDto.AreasOfLife;

                    UserAreaOfLifeUserHabit[] targetAreasAndHabits = DbContext.UserAreasOfLifeUserHabits
                        .Where( u => u.HabitId == habit.Id ).ToArray();

                    await DbContext.UserAreasOfLifeUserHabits.MergeAsync(
                        targetAreasAndHabits,
                        sourceAreas,
                        ( areasToInsert ) =>
                        {
                            return areasToInsert.Select(
                                area => new UserAreaOfLifeUserHabit() { AreaOfLifeId = area.Id, HabitId = habit.Id }
                            );
                        },
                        targetIdProp: "AreaOfLifeId"
                    ).DefaultConfigureAwait();

                }

                if (isNewHabit)
                {
                    await DbContext.UserHabits.AddOrUpdateAsync( habit ).DefaultConfigureAwait();
                }
                else
                {
                    userHabitList.Remove( habit );
                }

                await DbContext.Frequencies.AddOrUpdateAsync( habit.Frequency ).DefaultConfigureAwait();
                DbContext.UserHabits.UpdateRange( userHabitList );
                await DbContext.SaveChangesAsync().DefaultConfigureAwait();

                if (habitDto.Reminders?.Any() == true)
                {
                    TrackingOfUserNotificationRequests notificationRequest =
                        await m_reminderService.GetNotificationTrackingAsync( user.Id ).DefaultConfigureAwait();

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
                        await DbContext.UserHabitReminders.AddAsync( new UserHabitReminder
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
                }
                else
                {
                    UserHabitReminderDto[] sourceReminders =
                        habitDto.Reminders?.ToArray() ?? Array.Empty<UserHabitReminderDto>();

                    UserHabitReminder[] targetReminders = await DbContext.UserHabitReminders
                        .Where( u => u.UserHabitId == habit.Id )
                        .Include( r => r.DaysOfWeek )
                        .ToArrayAsync()
                        .ConfigureAwait( false );

                    await DbContext.UserHabitReminders.MergeAsync(
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
                        UserHabitReminderDto sourceReminder =
                            sourceReminders.FirstOrDefault( r => r.Id == targetReminder.Id );
                        if (sourceReminder != null)
                        {
                            await DbContext.WeekDays.MergeAsync(
                                targetReminder.DaysOfWeek.ToArray(),
                                sourceReminder.DaysOfWeek,
                                daysToInsert => daysToInsert.Select( d => new WeekDay
                                {
                                    Type = d.Type,
                                    UserNotificationRequestId = d.UserNotificationRequestId,
                                    UserHabitReminderId = targetReminder.Id
                                } ),
                                ( existingDay, updatedDay ) =>
                                {
                                    existingDay.Type = updatedDay.Type;
                                    existingDay.UserNotificationRequestId = updatedDay.UserNotificationRequestId;
                                }
                            ).DefaultConfigureAwait();
                        }
                    }

                    await DbContext.SaveChangesAsync().DefaultConfigureAwait();
                }

                await DbContext.SaveChangesAsync().DefaultConfigureAwait();

                List<ReminderIds>? reminderDetails = await DbContext.UserHabitReminders
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
                
                var result = new { habit.Id, habit.FrequencyId, ReminderIds = reminderDetails };

                return Ok( result );
            }
            catch
            {
                await transaction.RollbackAsync().DefaultConfigureAwait();
                throw;
            }
        } );
    }

    [HttpPut("priorities")]
    public Task<IActionResult> ResetPrioritiesAsync( [FromBody] List<UserHabitWithPriority> habits )
    {
        return TryCatchAsync( async (user) =>
        {
            #region Check param
            if (habits == null || habits.Count < 2)
            {
                return BadRequest( error: "Habits with priorities are less than 2" );
            }
            #endregion

            List<UserHabit> userHabitList = await DbContext.
                UserHabits.
                Where( h => h.UserId == user.Id ).
                ToListAsync().
                DefaultConfigureAwait();

            foreach (UserHabit userHabit in userHabitList)
            {
                int updatedPriority = habits.Find( h => h.Id == userHabit.Id )!.Priority;
                userHabit.Priority = updatedPriority;
            }

            await DbContext.SaveChangesAsync().DefaultConfigureAwait();
            OkResult result = Ok();

            return result;
        } );
    }

    [HttpDelete("{habitId}")]
    public Task<IActionResult> Delete( long habitId )
    {
        return TryCatchAsync( async () =>
        {
            #region Check parameter
            if (habitId == 0)
            {
                return BadRequest( "HabitIdIsZero" );
            }
            #endregion

            UserHabit? habit = habitId == 0
                ? null
                : await DbContext.UserHabits.FindAsync( habitId ).DefaultConfigureAwait();

            bool isCorrectArg = habit != null;

            IActionResult result;
            if (isCorrectArg)
            {
                long[] reminderIds = await DbContext.UserHabitReminders.Where( r => r.UserHabitId == habitId ).Select( r => r.Id ).ToArrayAsync().DefaultConfigureAwait();
                List<HabitDeletionResponse.NotificationRequest> notificationRequests = new();
                foreach (long idOfReminder in reminderIds)
                {
                    List<WeekDay> weekDaysOfReminder = await DbContext.WeekDays.Where( w => w.UserHabitReminderId == idOfReminder ).ToListAsync().DefaultConfigureAwait();
                    notificationRequests.AddRange( weekDaysOfReminder.Select( w => new HabitDeletionResponse.NotificationRequest( w.UserNotificationRequestId ) ) );
                }
                
                HabitDeletionResponse response = new();
                response.DeletedNotifications = notificationRequests;
                
                await using IDbContextTransaction transaction = await DbContext.Database.BeginTransactionAsync().DefaultConfigureAwait();

                try
                {
                    await DbContext.UserAreasOfLifeUserHabits.Where( p => p.HabitId == habitId ).ExecuteDeleteAsync()
                        .DefaultConfigureAwait();
                    await DbContext.UserHabits.Where( u => u.Id == habitId ).ExecuteDeleteAsync()
                        .DefaultConfigureAwait();
                    await DbContext.Frequencies.Where( f => f.Id == habit.FrequencyId ).ExecuteDeleteAsync()
                        .DefaultConfigureAwait();

                    await transaction.CommitAsync().DefaultConfigureAwait();
                }
                catch
                {
                    await transaction.RollbackAsync().DefaultConfigureAwait();
                    throw;
                }

                result = Ok(response);
            }
            else
            {
                result = BadRequest( $"HabitIsNotFoundWithId {habitId}" );
            }

            return result;
        } );
    }
}
