using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using SET.Shared.Models;
using SET.WebAPI.Models;

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
        return TryCatchAsync( _ => Task.FromResult<IActionResult>( Ok() ) );
    }

    [HttpGet( "bootstrap" )]
    public Task<IActionResult> BootstrapAsync()
    {
        return TryCatchAsync( async user =>
        {
            List<UserGoalDto> goals = await DbContext.UserGoals
                .Where( g => g.UserId == user.Id )
                .Select( g => new UserGoalDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    LastModified = g.UpdatedAt ?? g.CreatedAt
                } )
                .ToListAsync()
                .DefaultConfigureAwait();

            List<UserHabit> activeHabits = await DbContext.UserHabits
                .Where( u => u.Status == StatusOfHabit.InProgress && u.UserId == user.Id && !u.IsArchived )
                .Include( u => u.Progresses )
                .Include( u => u.Frequency )
                .Include( u => u.Goal )
                .OrderBy( u => u.Priority )
                .AsSplitQuery()
                .ToListAsync()
                .DefaultConfigureAwait();

            List<UserHabitInProgressShortDto> activeDtos = new();
            foreach (UserHabit habit in activeHabits)
            {
                List<UserAreaOfLife> areasOfLife = await DbContext.UserAreasOfLife
                    .Include( u => u.Habits )
                    .Where( u => u.Habits.Any( up => up.HabitId == habit.Id ) )
                    .ToListAsync()
                    .DefaultConfigureAwait();

                UserHabitInProgressShortDto habitDto = Mapper.Map<UserHabitInProgressShortDto>( habit );
                habitDto.AreasOfLife = Mapper.Map<UserAreaOfLifeDto[]>( areasOfLife );
                habitDto.LastModified = habit.UpdatedAt ?? habit.CreatedAt;

                UserHabitReminder? reminder = await DbContext.UserHabitReminders
                    .Where( r => r.UserHabitId == habit.Id )
                    .Include( r => r.DaysOfWeek )
                    .FirstOrDefaultAsync()
                    .DefaultConfigureAwait();

                if (reminder is not null)
                {
                    habitDto.Reminders = new List<UserHabitReminderDto>
                    {
                        Mapper.Map<UserHabitReminderDto>( reminder )
                    };
                }

                activeDtos.Add( habitDto );
            }

            List<SyncBootstrapArchivedHabitDto> archivedHabits = await DbContext.UserHabits
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

            UserReminder? habitsReportReminder = await DbContext.UserReminders
                .Where( r => r.UserId == user.Id && r.UserNotificationRequestId == 1 )
                .FirstOrDefaultAsync()
                .ConfigureAwait( false );

            UserReminderDto reminderDto = Mapper.Map<UserReminderDto>( habitsReportReminder ) ?? new UserReminderDto();

            List<TaskItemDto> tasks = await DbContext.Tasks
                .Where( t => t.UserId == user.Id )
                .OrderBy( t => t.Date )
                .ThenBy( t => t.Time )
                .Select( t => new TaskItemDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Notes = t.Notes,
                    Date = t.Date,
                    Time = t.Time,
                    IsCompleted = t.IsCompleted
                } )
                .ToListAsync()
                .DefaultConfigureAwait();

            SyncBootstrapResponse snapshot = new()
            {
                User = new SyncBootstrapUserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    MainSlogan = user.MainSlogan,
                    Mission = user.Mission,
                    Email = user.Email,
                    Gender = user.Gender,
                    LastModified = user.CreatedAt
                },
                Goals = goals,
                ActiveHabits = activeDtos,
                ArchivedHabits = archivedHabits,
                HabitsReportReminder = reminderDto,
                Tasks = tasks
            };

            return Ok( snapshot );
        } );
    }
}
