namespace SET.WebAPI.Controllers;

[Route( template: "api/habits")]
[ApiController]
[Authorize( AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme )]
public class HabitController : BaseController
{
    public HabitController( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        //do nothing
    }

    [HttpGet( template: "inprogress" )]
    public Task<IActionResult> InProgressIndex()
    {
        return TryCatchAsync( async ( user ) =>
        {
            List<UserHabit> listOfHabits = await DbContext.UserHabits.
                Where( u => u.Status == StatusOfHabit.InProgress && u.UserId == user.Id ).
                Include( u => u.Progresses ).
                Include( u => u.Frequency ).
                Include( u => u.AreasOfLife ).
                Include( u => u.Goal ).
                Include( u => u.Reminders).
                ThenInclude( r => r.DaysOfWeek ).
                OrderBy( u => u.Priority ).
                AsSplitQuery().
                ToListAsync().
                DefaultConfigureAwait();

            List<UserHabitInProgressShortDto> resultData = Mapper.Map<List<UserHabitInProgressShortDto>>( listOfHabits );
            OkObjectResult result = Ok( resultData );
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
                resultData.AreasOfLife = Mapper.Map<List<UserAreaOfLifeDto>>( areasOfLife );

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

            List<UserHabit> userHabitList = await DbContext.
                UserHabits.
                Where( h => h.UserId == user.Id ).
                ToListAsync().
                DefaultConfigureAwait();

            foreach (UserHabit userHabit in userHabitList.Where( h => h.Id != habitDto.Id ))
            {
                int? updatedPriority = habitDto.PrioritizedHabits?.Find(h => h.Id == userHabit.Id )?.Priority;
                if(updatedPriority != null)
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
            }
            else
            {
                habit.Name = habitDto.Name;
                habit.FrequencyId = habitDto.Frequency.Id;
                habit.Frequency = Mapper.Map<Frequency>(habitDto.Frequency);
                habit.ColorName = habitDto.ColorName;
                habit.ReasonToFollow = habitDto.ReasonToFollow;
                habit.Description = habitDto.Description;
                habit.Question = habitDto.Question;
                habit.Complexity = habitDto.Complexity;
                habit.Type = habitDto.Type;
                habit.Priority = habitDto.Priority;
                habit.Goal = null;
                habit.GoalId = habitDto.Goal?.Id > 0 ? habitDto.Goal.Id : null;
            }

            if (isNewHabit)
            {
                if (habitDto.AreasOfLife?.Any() == true)
                {
                    List<UserAreaOfLifeDto> currentAreas = habitDto.AreasOfLife.ToList();
                    foreach (UserAreaOfLifeDto area in currentAreas)
                    {
                        await DbContext.UserAreasOfLifeUserHabits.AddAsync( new UserAreaOfLifeUserHabit
                        {
                            Habit = habit,
                            AreaOfLifeId = area.Id
                        } );
                    }
                }
            }
            else
            {
                IEnumerable<UserAreaOfLifeDto> sourceAreas = habitDto.AreasOfLife;
                UserAreaOfLifeUserHabit[] targetAreasAndHabits = DbContext.UserAreasOfLifeUserHabits.
                    Where( u => u.HabitId == habit.Id ).
                    ToArray();

                await DbContext.UserAreasOfLifeUserHabits.MergeAsync(
                    targetAreasAndHabits,
                    sourceAreas,
                    ( areasToInsert ) =>
                    {
                        return areasToInsert.Select(
                            area => new UserAreaOfLifeUserHabit()
                            {
                                AreaOfLifeId = area.Id,
                                HabitId = habit.Id
                            }
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
                    await GetNotificationTrackingAsync( user.Id ).DefaultConfigureAwait();

                foreach (WeekDayDto weekDay in habitDto.Reminders.SelectMany( r => r.DaysOfWeek ))
                {
                    if (weekDay.UserNotificationRequestId == 0)
                    {
                        weekDay.UserNotificationRequestId = ++notificationRequest.MaxNotificationRequestId;
                    }
                }
            }

            if (isNewHabit ||
                (habitDto.Reminders?.Any() == true && habitDto.Reminders?.All( r => r.Id == 0 ) == true))
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
                IEnumerable<UserHabitReminderDto> sourceReminders = habitDto.Reminders;
                UserHabitReminder[] targetReminders = await DbContext.UserHabitReminders
                    .Where( u => u.UserHabitId == habit.Id )
                    .Include( r => r.DaysOfWeek )
                    .ToArrayAsync()
                    .DefaultConfigureAwait();

                await DbContext.UserHabitReminders.MergeAsync(
                    targetReminders,
                    sourceReminders,
                    ( remindersToInsert ) =>
                    {
                        return remindersToInsert.Select( 
                            reminder => new UserHabitReminder()
                            {
                                Title = reminder.Title,
                                Description = reminder.Description,
                                Time = reminder.Time,
                                IsEnabled = reminder.IsEnabled,
                                UserHabitId = habit.Id
                            } );
                    },
                    targetIdProp: "Id"
                ).DefaultConfigureAwait();
                
                await DbContext.SaveChangesAsync().DefaultConfigureAwait();
                
                targetReminders = await DbContext.UserHabitReminders
                    .Where( u => u.UserHabitId == habit.Id )
                    .Include( r => r.DaysOfWeek )
                    .ToArrayAsync()
                    .DefaultConfigureAwait();

                foreach (UserHabitReminder targetReminder in targetReminders)
                {
                    //TODO: fix it because it won't work for new reminders
                    UserHabitReminderDto? sourceReminder = sourceReminders?.FirstOrDefault( r => r.Id == targetReminder.Id );
                    if (sourceReminder is not null)
                    {
                        await DbContext.WeekDays.MergeAsync(
                            targetReminder.DaysOfWeek.ToArray(),
                            sourceReminder.DaysOfWeek,
                            ( daysToInsert ) =>
                            {
                                return daysToInsert.Select( d => new WeekDay
                                {
                                    Type = d.Type,
                                    UserNotificationRequestId = d.UserNotificationRequestId,
                                    UserHabitReminderId = targetReminder.Id
                                } );
                            }
                        ).DefaultConfigureAwait();
                    }
                }
            }

            await DbContext.SaveChangesAsync().DefaultConfigureAwait();

            List<ReminderIds>? reminderDetails = await DbContext.UserHabitReminders
                 .Where( r => r.UserHabitId == habit.Id )
                 .Select( r => new ReminderIds
                 {
                     Id = r.Id,
                     DaysOfWeek = r.DaysOfWeek.Select( d => new WeekDayIds
                     {
                         Id = d.Id,
                         NotificationRequestId = d.UserNotificationRequestId
                     } ).ToList()
                 } )
                 .ToListAsync().DefaultConfigureAwait();

            var result = new
            {
                habit.Id,
                habit.FrequencyId,
                ReminderIds = reminderDetails
            };

            return Ok( result );
        } );
    }

    public async Task<TrackingOfUserNotificationRequests> GetNotificationTrackingAsync(long userId )
    {
        TrackingOfUserNotificationRequests? trackingOfNotifications = await DbContext.TrackingOfUserNotificationRequests.FirstOrDefaultAsync( u => u.UserId == userId ).DefaultConfigureAwait();

        if(trackingOfNotifications == null)
        {
            trackingOfNotifications = new TrackingOfUserNotificationRequests
            {
                UserId = userId,
                MaxNotificationRequestId = 99//not 100, because we will increment it
            };

            DbContext.TrackingOfUserNotificationRequests.Add( trackingOfNotifications );
        }

        return trackingOfNotifications;
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

            IActionResult result;
            foreach (UserHabit userHabit in userHabitList)
            {
                int updatedPriority = habits.Find( h => h.Id == userHabit.Id )!.Priority;
                userHabit.Priority = updatedPriority;
            }

            await DbContext.SaveChangesAsync().DefaultConfigureAwait();
            result = Ok();

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
                await DbContext.ProgressesOfHabits.Where( p => p.HabitId == habitId ).ExecuteDeleteAsync();
                await DbContext.UserAreasOfLifeUserHabits.Where( p => p.HabitId == habitId ).ExecuteDeleteAsync();
                await DbContext.UserHabits.Where( u => u.Id == habitId ).ExecuteDeleteAsync();
                await DbContext.Frequencies.Where( f => f.Id == habit.FrequencyId ).ExecuteDeleteAsync();
                await DbContext.UserHabitReminders.Where( r => r.UserHabitId == habitId ).ExecuteDeleteAsync();

                result = Ok();
            }
            else
            {
                result = BadRequest( $"HabitIsNotFoundWithId {habitId}" );
            }

            return result;
        } );
    }
}
