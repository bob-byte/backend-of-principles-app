namespace SET.WebAPI.Controllers;

[Route( "api/sync" )]
[ApiController]
[Authorize( AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme )]
public class SyncController : BaseController
{
    public SyncController( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
    }

    [HttpGet( "ping" )]
    public Task<IActionResult> PingAsync()
    {
        return TryCatchAsync( user => Task.FromResult<IActionResult>( Ok( new { status = "ok", userId = user.Id } ) ) );
    }

    [HttpGet( "bootstrap" )]
    public Task<IActionResult> BootstrapAsync()
    {
        return TryCatchAsync( async user =>
        {
            List<UserGoal> goals = await DbContext.UserGoals
                .Where( goal => goal.UserId == user.Id )
                .OrderBy( goal => goal.Name )
                .ToListAsync()
                .DefaultConfigureAwait();

            List<UserHabit> habits = await DbContext.UserHabits
                .Where( habit => habit.UserId == user.Id )
                .Include( habit => habit.Progresses )
                .Include( habit => habit.Frequency )
                .Include( habit => habit.Goal )
                .Include( habit => habit.Reminders )
                    .ThenInclude( reminder => reminder.DaysOfWeek )
                .AsSplitQuery()
                .ToListAsync()
                .DefaultConfigureAwait();

            UserReminder? habitsReportReminder = await DbContext.UserReminders
                .Where( reminder => reminder.UserId == user.Id && reminder.UserNotificationRequestId == 1 )
                .FirstOrDefaultAsync()
                .DefaultConfigureAwait();

            SyncBootstrapResponse response = new()
            {
                User = new SyncUserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    MainSlogan = user.MainSlogan,
                    Mission = user.Mission,
                    Email = user.Email,
                    Gender = user.Gender,
                        LastModified = NormalizeStoredTimestamp( user.UpdatedAt, user.CreatedAt )
                },
                Goals = goals.Select( goal => new SyncGoalDto
                {
                    Id = goal.Id,
                    Name = goal.Name,
                    LastModified = NormalizeStoredTimestamp( goal.UpdatedAt ?? default, goal.CreatedAt )
                } ).ToList(),
                ActiveHabits = await BuildHabitDtosAsync( habits.Where( habit => !habit.IsArchived ).ToList() ).ConfigureAwait( false ),
                ArchivedHabits = await BuildHabitDtosAsync( habits.Where( habit => habit.IsArchived ).ToList() ).ConfigureAwait( false ),
                HabitsReportReminder = habitsReportReminder is null
                    ? null
                    : new SyncReminderDto
                    {
                        Id = habitsReportReminder.Id,
                        Title = habitsReportReminder.Title,
                        Description = habitsReportReminder.Description,
                        Time = habitsReportReminder.Time,
                        IsEnabled = habitsReportReminder.IsEnabled,
                        UserNotificationRequestId = habitsReportReminder.UserNotificationRequestId,
                        LastModified = NormalizeStoredTimestamp( habitsReportReminder.UpdatedAt, DateTime.UtcNow )
                    }
            };

            return Ok( response );
        } );
    }

    private async Task<List<SyncHabitDto>> BuildHabitDtosAsync( IReadOnlyCollection<UserHabit> habits )
    {
        List<SyncHabitDto> result = new( habits.Count );

        foreach (UserHabit habit in habits.OrderBy( h => h.Priority ).ThenBy( h => h.Name ))
        {
            DateTime habitLastModified = NormalizeStoredTimestamp( habit.UpdatedAt ?? default, habit.CreatedAt );
            Frequency? frequency = habit.Frequency;
            List<UserAreaOfLife> areasOfLife = await DbContext.UserAreasOfLife
                .Include( area => area.Habits )
                .Where( area => area.Habits.Any( link => link.HabitId == habit.Id ) )
                .OrderBy( area => area.Name )
                .ToListAsync()
                .DefaultConfigureAwait();

            SyncHabitDto dto = new()
            {
                Id = habit.Id,
                Name = habit.Name,
                Type = habit.Type,
                Status = habit.Status,
                Priority = habit.Priority,
                IsArchived = habit.IsArchived,
                Description = habit.Description,
                ArchivingTime = habit.ArchivingTime,
                ColorName = habit.ColorName,
                Complexity = habit.Complexity,
                LastModified = habitLastModified,
                Frequency = new SyncFrequencyDto
                {
                    Id = frequency?.Id ?? 0,
                    Type = frequency?.Type ?? FrequencyType.EveryDay,
                    Repeats = frequency is not null && frequency.Repeats > 0 ? frequency.Repeats : 1,
                    IntervalLengthInDays = frequency is not null && frequency.IntervalLengthInDays > 0 ? frequency.IntervalLengthInDays : 1,
                    LastModified = habitLastModified
                },
                Goal = habit.Goal is null
                    ? null
                    : new SyncGoalDto
                    {
                        Id = habit.Goal.Id,
                        Name = habit.Goal.Name,
                        LastModified = NormalizeStoredTimestamp( habit.Goal.UpdatedAt ?? default, habit.Goal.CreatedAt )
                    },
                AreasOfLife = areasOfLife.Select( area => new SyncAreaOfLifeDto
                {
                    Id = area.Id,
                    Name = area.Name,
                    LastModified = habitLastModified
                } ).ToList(),
                Reminders = habit.Reminders.Select( reminder => new SyncHabitReminderDto
                {
                    Id = reminder.Id,
                    Title = reminder.Title,
                    Description = reminder.Description,
                    Time = reminder.Time,
                    IsEnabled = reminder.IsEnabled,
                    LastModified = habitLastModified,
                    DaysOfWeek = reminder.DaysOfWeek
                        .OrderBy( day => day.Type )
                        .Select( day => new SyncWeekDayDto
                        {
                            Id = day.Id,
                            Type = day.Type,
                            UserNotificationRequestId = day.UserNotificationRequestId,
                            LastModified = habitLastModified
                        } )
                        .ToList()
                } ).ToList(),
                Progresses = habit.Progresses
                    .OrderByDescending( progress => progress.Date )
                    .Select( progress => new SyncProgressDto
                    {
                        Id = progress.Id,
                        Date = progress.Date,
                        Value = progress.Value,
                        LastModified = NormalizeStoredTimestamp( progress.UpdatedAt, habitLastModified )
                    } )
                    .ToList()
            };

            result.Add( dto );
        }

        return result;
    }
}
