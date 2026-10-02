using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using SET.Shared.Models;
using SET.WebAPI.Helpers;
using SET.WebAPI.Models;
using TaskEntity = SET.Shared.Models.Task;

namespace SET.WebAPI.Controllers;

[Route( "api/sync" )]
[ApiController]
[Authorize( AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme )]
public class SyncController : BaseController
{
    /// <summary>Peers older than this must call full bootstrap (tombstones may be gone).</summary>
    private static readonly TimeSpan MaxChangesAge = TimeSpan.FromDays( 14 );

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
            DateTime serverTime = DateTime.UtcNow;
            SyncBootstrapResponse snapshot = await BuildFullBootstrapAsync( user, serverTime )
                .DefaultConfigureAwait();
            return Ok( snapshot );
        } );
    }

    /// <summary>
    /// Incremental catch-up since the client's last successful sync cursor.
    /// Full bootstrap remains available for first sign-in / cold start.
    /// </summary>
    [HttpGet( "changes" )]
    public Task<IActionResult> ChangesAsync( [FromQuery] DateTime? since )
    {
        return TryCatchAsync( async user =>
        {
            DateTime serverTime = DateTime.UtcNow;
            if (since is null
                || since.Value.Kind == DateTimeKind.Unspecified && since.Value == default
                || serverTime - since.Value.ToUniversalTime() > MaxChangesAge)
            {
                return Ok( new SyncChangesResponse
                {
                    ServerTime = serverTime,
                    RequiresFullBootstrap = true,
                } );
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
                    LastModified = userStamp,
                };
            }

            List<UserGoalDto> goals = await DbContext.UserGoals
                .Where( g => g.UserId == user.Id
                             && ( ( g.UpdatedAt ?? g.ArchivingTime ?? g.CreatedAt ) > sinceUtc ) )
                .Select( g => new UserGoalDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    Notes = g.Notes,
                    IsCompleted = g.IsCompleted,
                    IsArchived = g.IsArchived,
                    LastModified = g.UpdatedAt ?? g.ArchivingTime ?? g.CreatedAt
                } )
                .ToListAsync()
                .DefaultConfigureAwait();

            List<UserHabit> changedActive = await DbContext.UserHabits
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

            List<UserHabitInProgressShortDto> activeDtos = new();
            foreach (UserHabit habit in changedActive)
            {
                activeDtos.Add( await MapActiveHabitDtoAsync( habit ).DefaultConfigureAwait() );
            }

            List<SyncBootstrapArchivedHabitDto> archivedHabits = await DbContext.UserHabits
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

            List<TaskEntity> changedTasks = await DbContext.Tasks
                .AsNoTracking()
                .Include( t => t.Subtasks )
                .Where( t => t.UserId == user.Id && t.UpdatedAt > sinceUtc )
                .OrderBy( t => t.Date )
                .ThenBy( t => t.Time )
                .ToListAsync()
                .DefaultConfigureAwait();

            List<AiConversation> changedConversations = await DbContext.AiConversations
                .AsNoTracking()
                .Include( c => c.Messages )
                .Where( c => c.UserId == user.Id && c.UpdatedAt > sinceUtc )
                .OrderByDescending( c => c.UpdatedAt )
                .ToListAsync()
                .DefaultConfigureAwait();

            // Reminders are small; always send the current lists so peers stay aligned
            // without reminder timestamps / tombstones.
            ( UserReminderDto habitsReport, List<UserReminderDto> general, List<UserHabitReminderDto> habitReminders )
                = await LoadReminderListsAsync( user ).DefaultConfigureAwait();

            List<SyncDeletion> deletions = await DbContext.SyncDeletions
                .AsNoTracking()
                .Where( d => d.UserId == user.Id && d.DeletedAt > sinceUtc )
                .ToListAsync()
                .DefaultConfigureAwait();

            return Ok( new SyncChangesResponse
            {
                ServerTime = serverTime,
                RequiresFullBootstrap = false,
                User = userDto,
                Goals = goals,
                ActiveHabits = activeDtos,
                ArchivedHabits = archivedHabits,
                HabitsReportReminder = habitsReport,
                GeneralReminders = general,
                UserHabitReminders = habitReminders,
                Tasks = changedTasks.Select( TaskDtoMapper.ToDto ).ToList(),
                Conversations = changedConversations
                    .Select( c => AiConversationDtoMapper.ToDto( c ) )
                    .ToList(),
                DeletedGoalIds = deletions
                    .Where( d => d.EntityType == SyncEntityTypes.Goal )
                    .Select( d => d.EntityId )
                    .Distinct()
                    .ToList(),
                DeletedHabitIds = deletions
                    .Where( d => d.EntityType == SyncEntityTypes.Habit )
                    .Select( d => d.EntityId )
                    .Distinct()
                    .ToList(),
                DeletedTaskIds = deletions
                    .Where( d => d.EntityType == SyncEntityTypes.Task )
                    .Select( d => d.EntityId )
                    .Distinct()
                    .ToList(),
                DeletedConversationIds = deletions
                    .Where( d => d.EntityType == SyncEntityTypes.Conversation )
                    .Select( d => d.EntityId )
                    .Distinct()
                    .ToList(),
            } );
        } );
    }

    private async Task<SyncBootstrapResponse> BuildFullBootstrapAsync( User user, DateTime serverTime )
    {
        List<UserGoalDto> goals = await DbContext.UserGoals
            .Where( g => g.UserId == user.Id )
            .Select( g => new UserGoalDto
            {
                Id = g.Id,
                Name = g.Name,
                Notes = g.Notes,
                IsCompleted = g.IsCompleted,
                IsArchived = g.IsArchived,
                LastModified = g.UpdatedAt ?? g.ArchivingTime ?? g.CreatedAt
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
            activeDtos.Add( await MapActiveHabitDtoAsync( habit ).DefaultConfigureAwait() );
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

        ( UserReminderDto habitsReport, List<UserReminderDto> general, List<UserHabitReminderDto> habitReminders )
            = await LoadReminderListsAsync( user ).DefaultConfigureAwait();

        List<TaskEntity> taskEntities = await DbContext.Tasks
            .AsNoTracking()
            .Include( t => t.Subtasks )
            .Where( t => t.UserId == user.Id )
            .OrderBy( t => t.Date )
            .ThenBy( t => t.Time )
            .ToListAsync()
            .DefaultConfigureAwait();

        List<AiConversation> conversationEntities = await DbContext.AiConversations
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
                LastModified = user.UpdatedAt ?? user.CreatedAt
            },
            Goals = goals,
            ActiveHabits = activeDtos,
            ArchivedHabits = archivedHabits,
            HabitsReportReminder = habitsReport,
            GeneralReminders = general,
            UserHabitReminders = habitReminders,
            Tasks = taskEntities.Select( TaskDtoMapper.ToDto ).ToList(),
            Conversations = conversationEntities
                .Select( c => AiConversationDtoMapper.ToDto( c ) )
                .ToList(),
        };
    }

    private async Task<UserHabitInProgressShortDto> MapActiveHabitDtoAsync( UserHabit habit )
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

        return habitDto;
    }

    private async Task<( UserReminderDto HabitsReport, List<UserReminderDto> General, List<UserHabitReminderDto> HabitReminders )>
        LoadReminderListsAsync( User user )
    {
        UserReminder? habitsReportReminder = await DbContext.UserReminders
            .Where( r => r.UserId == user.Id && r.UserNotificationRequestId == 1 )
            .FirstOrDefaultAsync()
            .ConfigureAwait( false );

        UserReminderDto reminderDto = Mapper.Map<UserReminderDto>( habitsReportReminder ) ?? new UserReminderDto();

        List<UserReminder> generalReminders = await DbContext.UserReminders
            .Where( r => r.UserId == user.Id )
            .ToListAsync()
            .ConfigureAwait( false );

        List<UserHabitReminder> habitReminders = await DbContext.UserHabitReminders
            .Where( r => r.UserHabit.User.Id == user.Id )
            .Include( r => r.DaysOfWeek )
            .ToListAsync()
            .ConfigureAwait( false );

        List<UserReminderDto> generalReminderDtos =
            Mapper.Map<List<UserReminderDto>>( generalReminders ) ?? new List<UserReminderDto>();
        List<UserHabitReminderDto> habitReminderDtos =
            Mapper.Map<List<UserHabitReminderDto>>( habitReminders ) ?? new List<UserHabitReminderDto>();

        return ( reminderDto, generalReminderDtos, habitReminderDtos );
    }
}
