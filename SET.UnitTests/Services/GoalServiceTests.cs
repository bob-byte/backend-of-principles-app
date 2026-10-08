using BusinessLogic;
using BusinessLogic.Models;
using Microsoft.EntityFrameworkCore;
using SET.DataAccess;
using SET.Shared.Models;
using SET.UnitTests.TestSupport;

namespace SET.UnitTests.Services;

public class GoalServiceTests
{
    private static (GoalService service, AppDbContext db, RecordingSyncPushService push) CreateSut()
    {
        AppDbContext db = TestDb.Create();
        RecordingSyncPushService push = new();
        return (new GoalService( db, push ), db, push);
    }

    [Fact]
    public async Task GetActiveAsync_ArchivedAndForeignGoals_ExcludesThem()
    {
        (GoalService service, AppDbContext db, _) = CreateSut();
        DateTime updated = new( 2026, 5, 1, 0, 0, 0, DateTimeKind.Utc );
        await TestData.AddGoalAsync( db, 1, "Active", g => g.UpdatedAt = updated );
        await TestData.AddGoalAsync( db, 1, "Archived", g => g.IsArchived = true );
        await TestData.AddGoalAsync( db, 2, "Foreign" );

        List<UserGoalDto> goals = await service.GetActiveAsync( 1 );

        UserGoalDto goal = Assert.Single( goals );
        Assert.Equal( "Active", goal.Name );
        Assert.Equal( updated, goal.LastModified );
    }

    [Fact]
    public async Task GetArchivedAsync_MultipleArchived_ReturnsNewestFirst()
    {
        (GoalService service, AppDbContext db, _) = CreateSut();
        DateTime archived = new( 2026, 4, 1, 0, 0, 0, DateTimeKind.Utc );
        UserGoal first = await TestData.AddGoalAsync( db, 1, "First", g =>
        {
            g.IsArchived = true;
            g.ArchivingTime = archived;
            g.Notes = "Keep these notes";
        } );
        UserGoal second = await TestData.AddGoalAsync( db, 1, "Second", g => g.IsArchived = true );
        await TestData.AddGoalAsync( db, 1, "Active" );

        List<ArchivedGoalResponse> goals = await service.GetArchivedAsync( 1 );

        Assert.Equal( new[] { second.Id, first.Id }, goals.Select( g => g.Id ) );
        Assert.Equal( archived, goals[1].LastModified );
        Assert.Equal( "Keep these notes", goals[1].Notes );
        Assert.Null( goals[0].Notes );
    }

    [Fact]
    public async Task SetArchiveStatusAsync_InvalidInput_ReturnsBadRequest()
    {
        (GoalService service, _, RecordingSyncPushService push) = CreateSut();

        TestData.AssertError( await service.SetArchiveStatusAsync( 1, null!, null ), 400, "GoalArchiveStatusIsNull" );
        TestData.AssertError( await service.SetArchiveStatusAsync( 1, new GoalArchiveStatus(), null ), 400, "GoalIdIsZero" );
        TestData.AssertError( await service.SetArchiveStatusAsync( 1, new GoalArchiveStatus { GoalId = 99 }, null ), 400, "GoalIsNotFound" );
        Assert.Empty( push.Requests );
    }

    [Fact]
    public async Task SetArchiveStatusAsync_ToggleArchive_UpdatesArchivingTimeAndNotifies()
    {
        (GoalService service, AppDbContext db, RecordingSyncPushService push) = CreateSut();
        UserGoal goal = await TestData.AddGoalAsync( db, 1, "Goal" );

        await service.SetArchiveStatusAsync( 1, new GoalArchiveStatus { GoalId = goal.Id, IsArchived = true }, "dev" );
        Assert.True( goal.IsArchived );
        Assert.NotNull( goal.ArchivingTime );
        Assert.Equal( goal.UpdatedAt, goal.ArchivingTime );

        await service.SetArchiveStatusAsync( 1, new GoalArchiveStatus { GoalId = goal.Id, IsArchived = false }, "dev" );
        Assert.False( goal.IsArchived );
        Assert.Null( goal.ArchivingTime );

        Assert.Equal( 2, push.Requests.Count );
        Assert.All( push.Requests, r => Assert.Equal( "dev", r.OriginDeviceId ) );
    }

    [Fact]
    public async Task SetArchiveStatusAsync_OtherUsersGoal_ReturnsNotFound()
    {
        (GoalService service, AppDbContext db, RecordingSyncPushService push) = CreateSut();
        UserGoal foreign = await TestData.AddGoalAsync( db, 2, "Foreign" );

        ServiceResult result = await service.SetArchiveStatusAsync(
            1, new GoalArchiveStatus { GoalId = foreign.Id, IsArchived = true }, null );

        TestData.AssertError( result, 400, "GoalIsNotFound" );
        Assert.False( foreign.IsArchived );
        Assert.Empty( push.Requests );
    }

    [Fact]
    public async Task DeleteAsync_OtherUsersGoal_ReturnsNotFound()
    {
        (GoalService service, AppDbContext db, _) = CreateSut();
        UserGoal foreign = await TestData.AddGoalAsync( db, 2, "Foreign" );

        TestData.AssertError( await service.DeleteAsync( 1, foreign.Id, null ), 400, "GoalIsNotFound" );
        Assert.Single( db.UserGoals );
        Assert.Empty( db.SyncDeletions );
    }

    [Fact]
    public async Task SaveAsync_OtherUsersGoal_ReturnsBadRequest()
    {
        (GoalService service, AppDbContext db, _) = CreateSut();
        User user = await TestData.AddUserAsync( db, 1 );
        UserGoal foreign = await TestData.AddGoalAsync( db, 2, "Foreign" );

        ServiceResult<DtoWithId> result = await service.SaveAsync( user, new UserGoalDto { Id = foreign.Id, Name = "Hijacked" }, null );

        TestData.AssertError( result, 400, $"GoalIsNotFoundWithId {foreign.Id}" );
        Assert.Equal( "Foreign", foreign.Name );
    }

    [Fact]
    public async Task DeleteAsync_ZeroOrUnknownId_ReturnsBadRequest()
    {
        (GoalService service, _, _) = CreateSut();

        TestData.AssertError( await service.DeleteAsync( 1, 0, null ), 400, "GoalIdIsZero" );
        TestData.AssertError( await service.DeleteAsync( 1, 404, null ), 400, "GoalIsNotFound" );
    }

    [Fact]
    public async Task SaveAsync_NewGoalWithNotes_CreatesTrimmedNotes()
    {
        (GoalService service, AppDbContext db, _) = CreateSut();
        User user = await TestData.AddUserAsync( db, 1 );

        ServiceResult<DtoWithId> result = await service.SaveAsync( user, new UserGoalDto
        {
            Name = "Run a marathon",
            Notes = "  Spring race  ",
            IsArchived = true,
        }, "dev" );

        UserGoal stored = await db.UserGoals.SingleAsync();
        Assert.Equal( stored.Id, result.Value!.Id );
        Assert.Equal( "Spring race", stored.Notes );
        Assert.Equal( 1, stored.UserId );
        Assert.NotNull( stored.ArchivingTime );
    }

    [Fact]
    public async Task SaveAsync_BlankNotes_StoresNull()
    {
        (GoalService service, AppDbContext db, _) = CreateSut();
        User user = await TestData.AddUserAsync( db, 1 );

        await service.SaveAsync( user, new UserGoalDto { Name = "Goal", Notes = "   " }, null );

        Assert.Null( (await db.UserGoals.SingleAsync()).Notes );
    }

    [Fact]
    public async Task SaveAsync_NullOrUnknownId_ReturnsBadRequest()
    {
        (GoalService service, AppDbContext db, _) = CreateSut();
        User user = await TestData.AddUserAsync( db, 1 );

        TestData.AssertError( await service.SaveAsync( user, null!, null ), 400, "UserGoalDtoIsNull" );
        TestData.AssertError( await service.SaveAsync( user, new UserGoalDto { Id = 77, Name = "x" }, null ), 400, "GoalIsNotFoundWithId 77" );
    }

    [Fact]
    public async Task SaveAsync_WithDeadlineAndReminders_PersistsThem()
    {
        (GoalService service, AppDbContext db, _) = CreateSut();
        User user = await TestData.AddUserAsync( db, 1 );

        ServiceResult<DtoWithId> result = await service.SaveAsync( user, new UserGoalDto
        {
            Name = "Ship release",
            Deadline = new DateOnly( 2026, 12, 31 ),
            Reminders = new List<TaskReminderOffsetDto>
            {
                new() { OffsetMinutes = 0 },
                new() { OffsetMinutes = 1440, NotificationRequestId = 42 },
            },
        }, null );

        UserGoal stored = await db.UserGoals.SingleAsync();
        Assert.Equal( stored.Id, result.Value!.Id );
        Assert.Equal( new DateOnly( 2026, 12, 31 ), stored.Deadline );
        Assert.Null( stored.DeadlineTime );
        Assert.Contains( "1440", stored.RemindersJson );
        Assert.Contains( "42", stored.RemindersJson );

        List<UserGoalDto> active = await service.GetActiveAsync( 1 );
        UserGoalDto dto = Assert.Single( active );
        Assert.Equal( new DateOnly( 2026, 12, 31 ), dto.Deadline );
        Assert.Equal( 2, dto.Reminders.Count );
    }

    [Fact]
    public async Task SaveAsync_ClearDeadline_ClearsReminders()
    {
        (GoalService service, AppDbContext db, _) = CreateSut();
        User user = await TestData.AddUserAsync( db, 1 );
        UserGoal goal = await TestData.AddGoalAsync( db, 1, "Goal", g =>
        {
            g.Deadline = new DateOnly( 2026, 6, 1 );
            g.RemindersJson = """[{"offsetMinutes":1440}]""";
        } );

        await service.SaveAsync( user, new UserGoalDto
        {
            Id = goal.Id,
            Name = "Goal",
            Deadline = null,
            Reminders = new List<TaskReminderOffsetDto>
            {
                new() { OffsetMinutes = 1440 },
            },
        }, null );

        Assert.Null( goal.Deadline );
        Assert.Null( goal.DeadlineTime );
        Assert.Null( goal.RemindersJson );
    }

    [Fact]
    public async Task SaveAsync_WithDeadlineTime_PersistsClock()
    {
        (GoalService service, AppDbContext db, _) = CreateSut();
        User user = await TestData.AddUserAsync( db, 1 );

        await service.SaveAsync( user, new UserGoalDto
        {
            Name = "Ship release",
            Deadline = new DateOnly( 2026, 12, 31 ),
            DeadlineTime = new TimeOnly( 15, 30 ),
        }, null );

        UserGoal stored = await db.UserGoals.SingleAsync();
        Assert.Equal( new TimeOnly( 15, 30 ), stored.DeadlineTime );

        await service.SaveAsync( user, new UserGoalDto
        {
            Id = stored.Id,
            Name = "Ship release",
            Deadline = new DateOnly( 2026, 12, 31 ),
            DeadlineTime = null,
        }, null );

        Assert.Null( stored.DeadlineTime );
        Assert.Equal( new DateOnly( 2026, 12, 31 ), stored.Deadline );
    }

    [Fact]
    public async Task SaveAsync_RenamedGoal_UpdatesMatchingReminderTitles()
    {
        (GoalService service, AppDbContext db, RecordingSyncPushService push) = CreateSut();
        User user = await TestData.AddUserAsync( db, 1 );
        UserGoal goal = await TestData.AddGoalAsync( db, 1, "Old goal" );
        UserHabit habit = await TestData.AddHabitAsync( db, 1, "Habit" );
        UserHabitReminder matching = await TestData.AddHabitReminderAsync( db, habit.Id, "Old goal" );
        UserHabitReminder other = await TestData.AddHabitReminderAsync( db, habit.Id, "Something else" );
        UserHabit foreignHabit = await TestData.AddHabitAsync( db, 2, "Foreign habit" );
        UserHabitReminder foreignSameTitle = await TestData.AddHabitReminderAsync( db, foreignHabit.Id, "Old goal" );

        ServiceResult<DtoWithId> result = await service.SaveAsync( user, new UserGoalDto
        {
            Id = goal.Id,
            Name = "New goal",
            IsCompleted = true,
        }, "dev" );

        Assert.Equal( goal.Id, result.Value!.Id );
        Assert.Equal( "New goal", goal.Name );
        Assert.True( goal.IsCompleted );
        Assert.NotNull( goal.UpdatedAt );
        Assert.Equal( "New goal", matching.Title );
        Assert.Equal( "Something else", other.Title );
        Assert.Equal( "Old goal", foreignSameTitle.Title );
        Assert.Equal( "dev", Assert.Single( push.Requests ).OriginDeviceId );
    }
}
