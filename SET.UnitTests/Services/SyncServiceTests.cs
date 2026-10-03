using BusinessLogic;
using BusinessLogic.Models;
using SET.DataAccess;
using SET.Shared.Models;
using SET.UnitTests.TestSupport;

namespace SET.UnitTests.Services;

public class SyncServiceTests
{
    private static readonly DateTime Old = DateTime.UtcNow.AddDays( -10 );
    private static readonly DateTime Since = DateTime.UtcNow.AddDays( -5 );
    private static readonly DateTime Recent = DateTime.UtcNow.AddDays( -1 );

    private static (SyncService service, AppDbContext db) CreateSut()
    {
        AppDbContext db = TestDb.Create();
        var mapper = BusinessLogicMapper.Create();
        return (new SyncService( db, mapper, new ReminderService( db, mapper ) ), db);
    }

    private static void AddDeletion( AppDbContext db, long userId, string type, long id, DateTime deletedAt )
    {
        db.SyncDeletions.Add( new SyncDeletion { UserId = userId, EntityType = type, EntityId = id, DeletedAt = deletedAt } );
    }

    [Fact]
    public async Task GetBootstrapAsync_MixedUsers_ReturnsOnlyOwnedData()
    {
        (SyncService service, AppDbContext db) = CreateSut();
        User user = await TestData.AddUserAsync( db, 1, configure: u => u.Mission = "Mission" );
        await TestData.AddUserAsync( db, 2 );
        await TestData.AddGoalAsync( db, 1, "Goal" );
        await TestData.AddGoalAsync( db, 2, "Foreign goal" );
        UserHabit active = await TestData.AddHabitAsync( db, 1, "Active" );
        await TestData.AddHabitAsync( db, 1, "Archived", h => h.IsArchived = true );
        await TestData.AddHabitAsync( db, 2, "Foreign habit" );
        await TestData.AddHabitReminderAsync( db, active.Id, "Ping", (DayOfWeek.Monday, 100) );
        await TestData.AddUserReminderAsync( db, 1, 1, title: "Report" );
        await TestData.AddTaskAsync( db, 1, "Task" );
        await TestData.AddTaskAsync( db, 2, "Foreign task" );

        SyncBootstrapResponse response = await service.GetBootstrapAsync( user );

        Assert.Equal( 1, response.User.Id );
        Assert.Equal( "Mission", response.User.Mission );
        Assert.Equal( user.CreatedAt, response.User.LastModified );
        Assert.Equal( "Goal", Assert.Single( response.Goals ).Name );
        UserHabitInProgressShortDto habit = Assert.Single( response.ActiveHabits );
        Assert.Equal( "Active", habit.Name );
        Assert.Equal( "Ping", Assert.Single( habit.Reminders ).Title );
        Assert.Equal( "Archived", Assert.Single( response.ArchivedHabits ).Name );
        Assert.Equal( "Report", response.HabitsReportReminder!.Title );
        Assert.Single( response.GeneralReminders );
        Assert.Single( response.UserHabitReminders );
        Assert.Equal( "Task", Assert.Single( response.Tasks ).Name );
        Assert.Empty( response.Conversations );
        Assert.True( response.ServerTime > DateTime.UtcNow.AddMinutes( -1 ) );
    }

    [Fact]
    public async Task GetChangesAsync_NoCursor_RequiresFullBootstrap()
    {
        (SyncService service, AppDbContext db) = CreateSut();
        User user = await TestData.AddUserAsync( db, 1 );

        Assert.True( (await service.GetChangesAsync( user, null )).RequiresFullBootstrap );
        Assert.True( (await service.GetChangesAsync( user, default( DateTime ) )).RequiresFullBootstrap );
    }

    [Fact]
    public async Task GetChangesAsync_StaleCursor_RequiresFullBootstrap()
    {
        (SyncService service, AppDbContext db) = CreateSut();
        User user = await TestData.AddUserAsync( db, 1 );

        SyncChangesResponse response = await service.GetChangesAsync( user, DateTime.UtcNow.AddDays( -15 ) );

        Assert.True( response.RequiresFullBootstrap );
        Assert.Empty( response.Goals );
    }

    [Fact]
    public async Task GetChangesAsync_RecentCursor_ReturnsChangedRowsOnly()
    {
        (SyncService service, AppDbContext db) = CreateSut();
        User user = await TestData.AddUserAsync( db, 1, configure: u => u.UpdatedAt = Old );
        await TestData.AddGoalAsync( db, 1, "Old goal", g => g.CreatedAt = Old );
        await TestData.AddGoalAsync( db, 1, "New goal", g => { g.CreatedAt = Old; g.UpdatedAt = Recent; } );
        await TestData.AddHabitAsync( db, 1, "Old habit", h => h.CreatedAt = Old );
        await TestData.AddHabitAsync( db, 1, "New habit", h => h.CreatedAt = Recent );
        await TestData.AddHabitAsync( db, 1, "Archived habit", h => { h.IsArchived = true; h.CreatedAt = Old; h.ArchivingTime = Recent; } );
        await TestData.AddTaskAsync( db, 1, "Old task", t => t.UpdatedAt = Old );
        await TestData.AddTaskAsync( db, 1, "New task", t => t.UpdatedAt = Recent );
        await TestData.AddTaskAsync( db, 2, "Foreign task", t => t.UpdatedAt = Recent );

        SyncChangesResponse response = await service.GetChangesAsync( user, Since );

        Assert.False( response.RequiresFullBootstrap );
        Assert.Null( response.User );
        Assert.Equal( "New goal", Assert.Single( response.Goals ).Name );
        Assert.Equal( "New habit", Assert.Single( response.ActiveHabits ).Name );
        Assert.Equal( "Archived habit", Assert.Single( response.ArchivedHabits ).Name );
        Assert.Equal( "New task", Assert.Single( response.Tasks ).Name );
    }

    [Fact]
    public async Task GetChangesAsync_ProfileChanged_IncludesProfile()
    {
        (SyncService service, AppDbContext db) = CreateSut();
        User user = await TestData.AddUserAsync( db, 1, configure: u => u.UpdatedAt = Recent );

        SyncChangesResponse response = await service.GetChangesAsync( user, Since );

        Assert.Equal( Recent, response.User!.LastModified );
    }

    [Fact]
    public async Task GetChangesAsync_RecentTombstones_GroupsDistinctIdsByType()
    {
        (SyncService service, AppDbContext db) = CreateSut();
        User user = await TestData.AddUserAsync( db, 1 );
        AddDeletion( db, 1, SyncEntityTypes.Task, 10, Recent );
        AddDeletion( db, 1, SyncEntityTypes.Task, 10, Recent );
        AddDeletion( db, 1, SyncEntityTypes.Task, 11, Old );
        AddDeletion( db, 1, SyncEntityTypes.Habit, 20, Recent );
        AddDeletion( db, 1, SyncEntityTypes.Goal, 30, Recent );
        AddDeletion( db, 1, SyncEntityTypes.Conversation, 40, Recent );
        AddDeletion( db, 2, SyncEntityTypes.Task, 50, Recent );
        await db.SaveChangesAsync();

        SyncChangesResponse response = await service.GetChangesAsync( user, Since );

        Assert.Equal( new long[] { 10 }, response.DeletedTaskIds );
        Assert.Equal( new long[] { 20 }, response.DeletedHabitIds );
        Assert.Equal( new long[] { 30 }, response.DeletedGoalIds );
        Assert.Equal( new long[] { 40 }, response.DeletedConversationIds );
    }

    [Fact]
    public async Task GetChangesAsync_AnyCursor_AlwaysSendsCurrentReminders()
    {
        (SyncService service, AppDbContext db) = CreateSut();
        User user = await TestData.AddUserAsync( db, 1 );
        UserHabit habit = await TestData.AddHabitAsync( db, 1, "Habit", h => h.CreatedAt = Old );
        await TestData.AddHabitReminderAsync( db, habit.Id, "Ping" );
        await TestData.AddUserReminderAsync( db, 1, 1 );

        SyncChangesResponse response = await service.GetChangesAsync( user, Since );

        Assert.Empty( response.ActiveHabits );
        Assert.Single( response.UserHabitReminders );
        Assert.Single( response.GeneralReminders );
        Assert.Equal( 1, response.HabitsReportReminder!.UserNotificationRequestId );
    }
}
