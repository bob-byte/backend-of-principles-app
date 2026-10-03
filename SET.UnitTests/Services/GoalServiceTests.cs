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
    public async Task GetActiveAsync_excludes_archived_and_other_users_goals()
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
    public async Task GetArchivedAsync_returns_newest_first_with_archive_time_fallback()
    {
        (GoalService service, AppDbContext db, _) = CreateSut();
        DateTime archived = new( 2026, 4, 1, 0, 0, 0, DateTimeKind.Utc );
        UserGoal first = await TestData.AddGoalAsync( db, 1, "First", g => { g.IsArchived = true; g.ArchivingTime = archived; } );
        UserGoal second = await TestData.AddGoalAsync( db, 1, "Second", g => g.IsArchived = true );
        await TestData.AddGoalAsync( db, 1, "Active" );

        List<ArchivedGoalResponse> goals = await service.GetArchivedAsync( 1 );

        Assert.Equal( new[] { second.Id, first.Id }, goals.Select( g => g.Id ) );
        Assert.Equal( archived, goals[1].LastModified );
    }

    [Fact]
    public async Task SetArchiveStatusAsync_validates_input()
    {
        (GoalService service, _, RecordingSyncPushService push) = CreateSut();

        TestData.AssertError( await service.SetArchiveStatusAsync( 1, null!, null ), 400, "GoalArchiveStatusIsNull" );
        TestData.AssertError( await service.SetArchiveStatusAsync( 1, new GoalArchiveStatus(), null ), 400, "GoalIdIsZero" );
        TestData.AssertError( await service.SetArchiveStatusAsync( 1, new GoalArchiveStatus { GoalId = 99 }, null ), 400, "GoalIsNotFound" );
        Assert.Empty( push.Requests );
    }

    [Fact]
    public async Task SetArchiveStatusAsync_archives_and_unarchives_with_archiving_time()
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
    public async Task SetArchiveStatusAsync_ignores_other_users_goal()
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
    public async Task DeleteAsync_ignores_other_users_goal()
    {
        (GoalService service, AppDbContext db, _) = CreateSut();
        UserGoal foreign = await TestData.AddGoalAsync( db, 2, "Foreign" );

        TestData.AssertError( await service.DeleteAsync( 1, foreign.Id ), 400, "GoalIsNotFound" );
        Assert.Single( db.UserGoals );
        Assert.Empty( db.SyncDeletions );
    }

    [Fact]
    public async Task SaveAsync_cannot_update_other_users_goal()
    {
        (GoalService service, AppDbContext db, _) = CreateSut();
        User user = await TestData.AddUserAsync( db, 1 );
        UserGoal foreign = await TestData.AddGoalAsync( db, 2, "Foreign" );

        ServiceResult<DtoWithId> result = await service.SaveAsync( user, new UserGoalDto { Id = foreign.Id, Name = "Hijacked" } );

        TestData.AssertError( result, 400, $"GoalIsNotFoundWithId {foreign.Id}" );
        Assert.Equal( "Foreign", foreign.Name );
    }

    [Fact]
    public async Task DeleteAsync_rejects_zero_and_unknown_ids()
    {
        (GoalService service, _, _) = CreateSut();

        TestData.AssertError( await service.DeleteAsync( 1, 0 ), 400, "GoalIdIsZero" );
        TestData.AssertError( await service.DeleteAsync( 1, 404 ), 400, "GoalIsNotFound" );
    }

    [Fact]
    public async Task SaveAsync_creates_goal_with_trimmed_notes()
    {
        (GoalService service, AppDbContext db, _) = CreateSut();
        User user = await TestData.AddUserAsync( db, 1 );

        ServiceResult<DtoWithId> result = await service.SaveAsync( user, new UserGoalDto
        {
            Name = "Run a marathon",
            Notes = "  Spring race  ",
            IsArchived = true,
        } );

        UserGoal stored = await db.UserGoals.SingleAsync();
        Assert.Equal( stored.Id, result.Value!.Id );
        Assert.Equal( "Spring race", stored.Notes );
        Assert.Equal( 1, stored.UserId );
        Assert.NotNull( stored.ArchivingTime );
    }

    [Fact]
    public async Task SaveAsync_blank_notes_are_stored_as_null()
    {
        (GoalService service, AppDbContext db, _) = CreateSut();
        User user = await TestData.AddUserAsync( db, 1 );

        await service.SaveAsync( user, new UserGoalDto { Name = "Goal", Notes = "   " } );

        Assert.Null( (await db.UserGoals.SingleAsync()).Notes );
    }

    [Fact]
    public async Task SaveAsync_rejects_null_and_unknown_ids()
    {
        (GoalService service, AppDbContext db, _) = CreateSut();
        User user = await TestData.AddUserAsync( db, 1 );

        TestData.AssertError( await service.SaveAsync( user, null! ), 400, "UserGoalDtoIsNull" );
        TestData.AssertError( await service.SaveAsync( user, new UserGoalDto { Id = 77, Name = "x" } ), 400, "GoalIsNotFoundWithId 77" );
    }

    [Fact]
    public async Task SaveAsync_update_renames_reminders_titled_with_old_goal_name()
    {
        (GoalService service, AppDbContext db, _) = CreateSut();
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
        } );

        Assert.Equal( goal.Id, result.Value!.Id );
        Assert.Equal( "New goal", goal.Name );
        Assert.True( goal.IsCompleted );
        Assert.NotNull( goal.UpdatedAt );
        Assert.Equal( "New goal", matching.Title );
        Assert.Equal( "Something else", other.Title );
        Assert.Equal( "Old goal", foreignSameTitle.Title );
    }
}
