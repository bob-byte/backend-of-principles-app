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

    [HttpGet( template: "inprogress/{userId}" )]
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
                    dtoReminder.DaysOfWeek = reminder.DaysOfWeek.Select( d => d.Type ).ToArray();

                    resultData.UserHabitReminders = new List<UserHabitReminderDto>
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
                //this block is an analog to MERGE operator which inserts and deletes

                IEnumerable<long> sourceAreasIds = habitDto.AreasOfLife.Select( s => s.Id );
                UserAreaOfLifeUserHabit[] targetAreasAndHabits = DbContext.UserAreasOfLifeUserHabits.
                    Where( u => u.HabitId == habit.Id ).
                    ToArray();

                //delete from database items that were removed by client
                IEnumerable<UserAreaOfLifeUserHabit> elemsNotFoundInSource = targetAreasAndHabits.Where( s => !sourceAreasIds.Contains( s.AreaOfLifeId ) );
                DbContext.UserAreasOfLifeUserHabits.RemoveRange( elemsNotFoundInSource );

                //insert new areas of life that was added by client
                long[] itemsThatNotExistInSource = sourceAreasIds.Where( sourceAreaId => !targetAreasAndHabits.Any( a => a.AreaOfLifeId == sourceAreaId ) ).ToArray();
                IEnumerable<UserAreaOfLifeUserHabit> toInsertAreas = itemsThatNotExistInSource.Select(
                    areaId => new UserAreaOfLifeUserHabit()
                    {
                        AreaOfLifeId = areaId,
                        HabitId = habit.Id
                    }
                );
                await DbContext.UserAreasOfLifeUserHabits.AddRangeAsync( toInsertAreas ).DefaultConfigureAwait();
            }

            if (isNewHabit)
            {
                if (habitDto.UserHabitReminders?.Any() == true)
                {
                    List<UserHabitReminderDto> currentReminders = habitDto.UserHabitReminders.ToList();
                    foreach (UserHabitReminderDto reminder in currentReminders)
                    {
                        await DbContext.UserHabitReminders.AddAsync( new UserHabitReminder
                        {
                            UserHabit = habit,
                            Title = reminder.Title,
                            Description = reminder.Description,
                            Time = reminder.Time,
                            IsEnabled = reminder.IsEnabled,
                            DaysOfWeek = reminder.DaysOfWeek.Select( day => new WeekDay
                            {
                                Type = day
                            } ).ToList()
                        } );
                    }
                }
            }
            else
            {
                IEnumerable<long> sourceRemindersIds = habitDto.UserHabitReminders is null
                    ? Enumerable.Empty<long>()
                    : habitDto.UserHabitReminders.Select( s => s.Id );

                UserHabitReminder[] targetReminders = DbContext.UserHabitReminders
                    .Where( u => u.UserHabitId == habit.Id )
                    .ToArray();

                IEnumerable<UserHabitReminder> remindersToDelete = targetReminders
                    .Where( s => !sourceRemindersIds.Contains( s.Id ) );
                DbContext.UserHabitReminders.RemoveRange( remindersToDelete );

                IEnumerable<UserHabitReminder> remindersToAdd = habitDto.UserHabitReminders is null
                    ? Enumerable.Empty<UserHabitReminder>()
                    : habitDto.UserHabitReminders
                    .Where( r => r.Id == 0 )
                    .Select( r => new UserHabitReminder
                    {
                        Title = r.Title,
                        Description = r.Description,
                        Time = r.Time,
                        IsEnabled = r.IsEnabled,
                        UserHabitId = habit.Id,
                        DaysOfWeek = r.DaysOfWeek.Select( day => new WeekDay
                        {
                            Type = day
                        } ).ToList()
                    } );

                await DbContext.UserHabitReminders.AddRangeAsync( remindersToAdd ).DefaultConfigureAwait();

                foreach (UserHabitReminderDto reminderDto in habitDto.UserHabitReminders.Where( r => r.Id != 0 ))
                {
                    UserHabitReminder existingReminder = targetReminders.FirstOrDefault( r => r.Id == reminderDto.Id );
                    if (existingReminder != null)
                    {
                        existingReminder.Title = reminderDto.Title;
                        existingReminder.Description = reminderDto.Description;
                        existingReminder.Time = reminderDto.Time;
                        existingReminder.IsEnabled = reminderDto.IsEnabled;
                        existingReminder.DaysOfWeek.Clear();
                        existingReminder.DaysOfWeek = reminderDto.DaysOfWeek.Select( day => new WeekDay
                        {
                            Type = day
                        } ).ToList();

                        DbContext.UserHabitReminders.Update( existingReminder );
                    }
                }
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

            List<long> reminderIds = DbContext.UserHabitReminders
                .Where( r => r.UserHabitId == habit.Id )
                .Select( r => r.Id )
                .ToList();

            var result = new
            {
                habit.Id,
                habit.FrequencyId,
                ReminderIds = reminderIds
            };

            return Ok( result );
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
