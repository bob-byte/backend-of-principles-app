using BusinessLogic;
using SET.DataAccess;
using SET.Shared.Models;

namespace SET.UnitTests.TestSupport;

/// <summary>Seed helpers that fill every required column so InMemory accepts the rows.</summary>
internal static class TestData
{
    public static async Task<User> AddUserAsync(
        AppDbContext db,
        long id,
        string? email = null,
        Action<User>? configure = null )
    {
        User user = new()
        {
            Id = id,
            Email = email ?? $"user{id}@example.com",
            Name = $"User {id}",
            Gender = Gender.Woman,
            CreatedAt = new DateTime( 2026, 1, 1, 0, 0, 0, DateTimeKind.Utc ),
        };
        configure?.Invoke( user );
        db.Users.Add( user );
        await db.SaveChangesAsync();
        return user;
    }

    public static async Task<UserGoal> AddGoalAsync(
        AppDbContext db,
        long userId,
        string name,
        Action<UserGoal>? configure = null )
    {
        UserGoal goal = new()
        {
            UserId = userId,
            Name = name,
            CreatedAt = new DateTime( 2026, 1, 1, 0, 0, 0, DateTimeKind.Utc ),
        };
        configure?.Invoke( goal );
        db.UserGoals.Add( goal );
        await db.SaveChangesAsync();
        return goal;
    }

    public static async Task<UserHabit> AddHabitAsync(
        AppDbContext db,
        long userId,
        string name,
        Action<UserHabit>? configure = null )
    {
        UserHabit habit = new()
        {
            UserId = userId,
            Name = name,
            ColorName = "#FFFFFF",
            Status = StatusOfHabit.InProgress,
            Frequency = new Frequency { Type = FrequencyType.EveryDay, Repeats = 1, IntervalLengthInDays = 1 },
            CreatedAt = new DateTime( 2026, 1, 1, 0, 0, 0, DateTimeKind.Utc ),
        };
        configure?.Invoke( habit );
        db.UserHabits.Add( habit );
        await db.SaveChangesAsync();
        return habit;
    }

    public static async Task<UserHabitReminder> AddHabitReminderAsync(
        AppDbContext db,
        long habitId,
        string title,
        params (DayOfWeek Day, int NotificationRequestId)[] days )
    {
        UserHabitReminder reminder = new()
        {
            UserHabitId = habitId,
            Title = title,
            Description = string.Empty,
            Time = new TimeOnly( 8, 0 ),
            IsEnabled = true,
            DaysOfWeek = days
                .Select( d => new WeekDay { Type = d.Day, UserNotificationRequestId = d.NotificationRequestId } )
                .ToList(),
        };
        db.UserHabitReminders.Add( reminder );
        await db.SaveChangesAsync();
        return reminder;
    }

    public static async Task<UserReminder> AddUserReminderAsync(
        AppDbContext db,
        long userId,
        int notificationRequestId,
        string title = "Report",
        string description = "Daily report" )
    {
        UserReminder reminder = new()
        {
            UserId = userId,
            Title = title,
            Description = description,
            Time = new TimeOnly( 21, 0 ),
            IsEnabled = true,
            UserNotificationRequestId = notificationRequestId,
        };
        db.UserReminders.Add( reminder );
        await db.SaveChangesAsync();
        return reminder;
    }

    public static async Task<UserAreaOfLife> AddAreaOfLifeAsync( AppDbContext db, long userId, string name )
    {
        UserAreaOfLife area = new() { UserId = userId, Name = name };
        db.UserAreasOfLife.Add( area );
        await db.SaveChangesAsync();
        return area;
    }

    public static async Task<SET.Shared.Models.Task> AddTaskAsync(
        AppDbContext db,
        long userId,
        string name,
        Action<SET.Shared.Models.Task>? configure = null )
    {
        SET.Shared.Models.Task task = new()
        {
            UserId = userId,
            Name = name,
            CreatedAt = new DateTime( 2026, 1, 1, 0, 0, 0, DateTimeKind.Utc ),
            UpdatedAt = new DateTime( 2026, 1, 1, 0, 0, 0, DateTimeKind.Utc ),
        };
        configure?.Invoke( task );
        db.Tasks.Add( task );
        await db.SaveChangesAsync();
        return task;
    }

    public static ServiceError AssertError( ServiceResult result, int statusCode, object body )
    {
        Assert.False( result.IsSuccess );
        Assert.NotNull( result.Error );
        Assert.Equal( statusCode, result.Error!.StatusCode );
        Assert.Equal( body, result.Error.Body );
        return result.Error;
    }

    /// <summary>Reads <c>error</c> from the anonymous <c>{ error }</c> body AI endpoints return.</summary>
    public static string? AiErrorMessage( ServiceError error )
    {
        return error.Body.GetType().GetProperty( "error" )?.GetValue( error.Body ) as string;
    }
}
