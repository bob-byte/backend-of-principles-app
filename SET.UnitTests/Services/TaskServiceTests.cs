using BusinessLogic;
using BusinessLogic.Models;
using Microsoft.EntityFrameworkCore;
using SET.DataAccess;
using SET.Shared.Models;
using SET.UnitTests.TestSupport;

namespace SET.UnitTests.Services;

public class TaskServiceTests
{
    private static (TaskService service, AppDbContext db, RecordingSyncPushService push) CreateSut()
    {
        AppDbContext db = TestDb.Create();
        RecordingSyncPushService push = new();
        return (new TaskService( db, push ), db, push);
    }

    [Theory]
    [InlineData( null, "NameIsNullOrWhiteSpace" )]
    [InlineData( "   ", "NameIsNullOrWhiteSpace" )]
    public async Task CreateAsync_rejects_blank_name( string? name, string token )
    {
        (TaskService service, _, RecordingSyncPushService push) = CreateSut();

        ServiceResult<TaskItemDto> result = await service.CreateAsync( 1, new TaskItemDto { Name = name! }, "dev" );

        TestData.AssertError( result, 400, token );
        Assert.Empty( push.Requests );
    }

    [Fact]
    public async Task CreateAsync_rejects_null_and_too_long_names()
    {
        (TaskService service, _, _) = CreateSut();

        TestData.AssertError( await service.CreateAsync( 1, null!, null ), 400, "TaskIsNull" );
        TestData.AssertError(
            await service.CreateAsync( 1, new TaskItemDto { Name = new string( 'a', 256 ) }, null ),
            400,
            "NameIsTooLong" );
    }

    [Fact]
    public async Task CreateAsync_persists_task_with_subtasks_and_notifies_other_devices()
    {
        (TaskService service, AppDbContext db, RecordingSyncPushService push) = CreateSut();

        ServiceResult<TaskItemDto> result = await service.CreateAsync( 1, new TaskItemDto
        {
            Name = "Buy milk",
            IsCompleted = true,
            Subtasks = new List<TaskSubtaskDto> { new() { Id = "s1", Name = "Oat" } },
        }, "dev-a" );

        Assert.True( result.IsSuccess );
        Assert.True( result.Value!.Id > 0 );
        Assert.Equal( "Buy milk", result.Value.Name );
        Assert.True( result.Value.IsCompleted );
        Assert.Equal( "Oat", Assert.Single( result.Value.Subtasks! ).Name );

        SET.Shared.Models.Task stored = await db.Tasks.Include( t => t.Subtasks ).SingleAsync();
        Assert.Equal( 1, stored.UserId );
        Assert.Single( stored.Subtasks );

        SyncPushRequest request = Assert.Single( push.Requests );
        Assert.Equal( 1, request.UserId );
        Assert.Equal( "dev-a", request.OriginDeviceId );
    }

    [Fact]
    public async Task GetByDateAsync_requires_date_and_returns_only_users_tasks_for_that_day_by_time()
    {
        (TaskService service, AppDbContext db, _) = CreateSut();
        DateOnly day = new( 2026, 10, 3 );
        await TestData.AddTaskAsync( db, 1, "Late", t => { t.Date = day; t.Time = new TimeOnly( 18, 0 ); } );
        await TestData.AddTaskAsync( db, 1, "Early", t => { t.Date = day; t.Time = new TimeOnly( 8, 0 ); } );
        await TestData.AddTaskAsync( db, 1, "Other day", t => t.Date = day.AddDays( 1 ) );
        await TestData.AddTaskAsync( db, 2, "Other user", t => t.Date = day );

        TestData.AssertError( await service.GetByDateAsync( 1, null ), 400, "DateIsNotSpecified" );

        ServiceResult<List<TaskItemDto>> result = await service.GetByDateAsync( 1, day );
        Assert.Equal( new[] { "Early", "Late" }, result.Value!.Select( t => t.Name ) );
    }

    [Fact]
    public async Task GetAllAsync_returns_only_users_tasks()
    {
        (TaskService service, AppDbContext db, _) = CreateSut();
        await TestData.AddTaskAsync( db, 1, "Mine" );
        await TestData.AddTaskAsync( db, 2, "Theirs" );

        List<TaskItemDto> tasks = await service.GetAllAsync( 1 );

        Assert.Equal( "Mine", Assert.Single( tasks ).Name );
    }

    [Fact]
    public async Task GetInboxAsync_returns_open_undated_or_upcoming_tasks()
    {
        (TaskService service, AppDbContext db, _) = CreateSut();
        DateOnly today = DateOnly.FromDateTime( DateTime.Today );
        await TestData.AddTaskAsync( db, 1, "Undated" );
        await TestData.AddTaskAsync( db, 1, "Today", t => t.Date = today );
        await TestData.AddTaskAsync( db, 1, "Past", t => t.Date = today.AddDays( -1 ) );
        await TestData.AddTaskAsync( db, 1, "Done", t => t.IsCompleted = true );

        List<TaskItemDto> tasks = await service.GetInboxAsync( 1 );

        Assert.Equal( new[] { "Today", "Undated" }, tasks.Select( t => t.Name ).Order() );
    }

    [Fact]
    public async Task UpdateStatusAsync_validates_and_scopes_to_user()
    {
        (TaskService service, AppDbContext db, RecordingSyncPushService push) = CreateSut();
        SET.Shared.Models.Task task = await TestData.AddTaskAsync( db, 2, "Theirs" );

        TestData.AssertError( await service.UpdateStatusAsync( 1, 0, new UpdateTaskStatusDto(), null ), 400, "TaskIdIsZeroOrNegative" );
        TestData.AssertError( await service.UpdateStatusAsync( 1, task.Id, null!, null ), 400, "RequestIsNull" );
        TestData.AssertError(
            await service.UpdateStatusAsync( 1, task.Id, new UpdateTaskStatusDto { IsCompleted = true }, null ),
            404,
            $"TaskIsNotFoundWithId {task.Id}" );
        Assert.Empty( push.Requests );
    }

    [Fact]
    public async Task UpdateStatusAsync_completes_task_and_bumps_updated_at()
    {
        (TaskService service, AppDbContext db, RecordingSyncPushService push) = CreateSut();
        SET.Shared.Models.Task task = await TestData.AddTaskAsync( db, 1, "Mine" );
        DateTime before = task.UpdatedAt;

        ServiceResult<TaskItemDto> result =
            await service.UpdateStatusAsync( 1, task.Id, new UpdateTaskStatusDto { IsCompleted = true }, "dev" );

        Assert.True( result.Value!.IsCompleted );
        Assert.True( task.UpdatedAt > before );
        Assert.Single( push.Requests );
    }

    [Fact]
    public async Task UpdateAsync_validates_then_applies_dto()
    {
        (TaskService service, AppDbContext db, RecordingSyncPushService push) = CreateSut();
        SET.Shared.Models.Task task = await TestData.AddTaskAsync( db, 1, "Old" );

        TestData.AssertError( await service.UpdateAsync( 1, -1, new TaskItemDto { Name = "x" }, null ), 400, "TaskIdIsZeroOrNegative" );
        TestData.AssertError( await service.UpdateAsync( 1, task.Id, null!, null ), 400, "RequestIsNull" );
        TestData.AssertError( await service.UpdateAsync( 1, task.Id, new TaskItemDto { Name = " " }, null ), 400, "NameIsNullOrWhiteSpace" );
        TestData.AssertError( await service.UpdateAsync( 2, task.Id, new TaskItemDto { Name = "x" }, null ), 404, $"TaskIsNotFoundWithId {task.Id}" );

        ServiceResult<TaskItemDto> result = await service.UpdateAsync( 1, task.Id, new TaskItemDto { Name = "New", Notes = "n" }, null );

        Assert.Equal( "New", result.Value!.Name );
        Assert.Equal( "n", (await db.Tasks.SingleAsync()).Notes );
        Assert.Single( push.Requests );
    }

    [Fact]
    public async Task DeleteAsync_removes_task_writes_tombstone_and_pushes_deleted_id()
    {
        (TaskService service, AppDbContext db, RecordingSyncPushService push) = CreateSut();
        SET.Shared.Models.Task task = await TestData.AddTaskAsync( db, 1, "Mine" );

        ServiceResult result = await service.DeleteAsync( 1, task.Id, "dev" );

        Assert.True( result.IsSuccess );
        Assert.Empty( db.Tasks );
        SyncDeletion tombstone = await db.SyncDeletions.SingleAsync();
        Assert.Equal( SyncEntityTypes.Task, tombstone.EntityType );
        Assert.Equal( task.Id, tombstone.EntityId );
        Assert.Equal( new[] { task.Id }, Assert.Single( push.Requests ).DeletedTaskIds );
    }

    [Fact]
    public async Task DeleteAsync_rejects_invalid_and_foreign_ids()
    {
        (TaskService service, AppDbContext db, _) = CreateSut();
        SET.Shared.Models.Task task = await TestData.AddTaskAsync( db, 2, "Theirs" );

        TestData.AssertError( await service.DeleteAsync( 1, 0, null ), 400, "TaskIdIsZeroOrNegative" );
        TestData.AssertError( await service.DeleteAsync( 1, task.Id, null ), 404, $"TaskIsNotFoundWithId {task.Id}" );
        Assert.Single( db.Tasks );
    }
}
