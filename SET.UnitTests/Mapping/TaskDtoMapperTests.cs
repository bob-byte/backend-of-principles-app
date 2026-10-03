using BusinessLogic;
using BusinessLogic.Models;
using SET.Shared.Models;
using TaskEntity = SET.Shared.Models.Task;

namespace SET.UnitTests.Mapping;

public class TaskDtoMapperTests
{
    [Fact]
    public void ApplySubtasks_NullList_KeepsExistingRows()
    {
        TaskEntity entity = new() { Subtasks = new List<TaskSubtask> { new() { ClientId = "a", Title = "Keep" } } };

        TaskDtoMapper.ApplySubtasks( entity, null );

        Assert.Equal( "Keep", Assert.Single( entity.Subtasks ).Title );
    }

    [Fact]
    public void ApplySubtasks_EmptyList_ClearsRows()
    {
        TaskEntity entity = new() { Subtasks = new List<TaskSubtask> { new() { ClientId = "a", Title = "Drop" } } };

        TaskDtoMapper.ApplySubtasks( entity, new List<TaskSubtaskDto>() );

        Assert.Empty( entity.Subtasks );
    }

    [Fact]
    public void ApplySubtasks_MixedTitles_SkipsBlankTrimsTruncatesOrders()
    {
        TaskEntity entity = new();
        string longTitle = new( 'x', 300 );
        string longId = new( 'i', 80 );

        TaskDtoMapper.ApplySubtasks( entity, new List<TaskSubtaskDto>
        {
            new() { Id = "first", Name = "  First  ", IsCompleted = true },
            new() { Id = "blank", Name = "   " },
            new() { Id = longId, Name = longTitle },
            new() { Id = "", Name = "Generated id" },
        } );

        List<TaskSubtask> subtasks = entity.Subtasks.ToList();
        Assert.Equal( 3, subtasks.Count );
        Assert.Equal( "First", subtasks[0].Title );
        Assert.True( subtasks[0].IsCompleted );
        Assert.Equal( 0, subtasks[0].SortOrder );
        Assert.Equal( 255, subtasks[1].Title.Length );
        Assert.Equal( 64, subtasks[1].ClientId.Length );
        Assert.Equal( 1, subtasks[1].SortOrder );
        Assert.Equal( 32, subtasks[2].ClientId.Length );
        Assert.Equal( 2, subtasks[2].SortOrder );
    }

    [Fact]
    public void ApplyDto_ValidDto_CopiesFieldsAndSerializesSchedule()
    {
        TaskEntity entity = new();
        TaskItemDto dto = new()
        {
            Name = "Pay rent",
            Notes = "Before noon",
            Date = new DateOnly( 2026, 10, 3 ),
            Time = new TimeOnly( 11, 0 ),
            AllDay = false,
            ConstantReminder = true,
            ConstantNotificationRequestId = 42,
            Reminders = new List<TaskReminderOffsetDto> { new() { OffsetMinutes = 15, NotificationRequestId = 7 } },
            Repeat = new TaskRepeatDto { Preset = "weekly", Weekdays = new[] { 1, 3 } },
        };

        TaskDtoMapper.ApplyDto( entity, dto );

        Assert.Equal( "Pay rent", entity.Name );
        Assert.Equal( "Before noon", entity.Notes );
        Assert.True( entity.ConstantReminder );
        Assert.Equal( 42, entity.ConstantNotificationRequestId );
        Assert.NotNull( entity.RemindersJson );
        Assert.NotNull( entity.RepeatJson );
        Assert.NotEqual( default, entity.CreatedAt );
        Assert.NotEqual( default, entity.UpdatedAt );

        TaskItemDto roundTrip = TaskDtoMapper.ToDto( entity );
        TaskReminderOffsetDto reminder = Assert.Single( roundTrip.Reminders );
        Assert.Equal( 15, reminder.OffsetMinutes );
        Assert.Equal( 7, reminder.NotificationRequestId );
        Assert.Equal( "weekly", roundTrip.Repeat!.Preset );
        Assert.Equal( new[] { 1, 3 }, roundTrip.Repeat.Weekdays );
    }

    [Fact]
    public void ApplyDto_NoneRepeatOrEmptyReminders_StoresNull()
    {
        Assert.Null( TaskDtoMapper.SerializeRepeat( new TaskRepeatDto { Preset = "None" } ) );
        Assert.Null( TaskDtoMapper.SerializeRepeat( null ) );
        Assert.Null( TaskDtoMapper.SerializeReminders( new List<TaskReminderOffsetDto>() ) );
        Assert.Null( TaskDtoMapper.SerializeReminders( null ) );
    }

    [Fact]
    public void Parse_InvalidJson_ParsesToDefaults()
    {
        Assert.Empty( TaskDtoMapper.ParseReminders( "not json" ) );
        Assert.Empty( TaskDtoMapper.ParseReminders( null ) );
        Assert.Null( TaskDtoMapper.ParseRepeat( "{broken" ) );
    }

    [Fact]
    public void ToDto_Subtasks_OrdersAndFallsBackToCreatedAt()
    {
        DateTime created = new( 2026, 1, 2, 0, 0, 0, DateTimeKind.Utc );
        TaskEntity entity = new()
        {
            Id = 5,
            Name = "Task",
            CreatedAt = created,
            Subtasks = new List<TaskSubtask>
            {
                new() { ClientId = "b", Title = "Second", SortOrder = 1 },
                new() { ClientId = "a", Title = "First", SortOrder = 0 },
            },
        };

        TaskItemDto dto = TaskDtoMapper.ToDto( entity );

        Assert.Equal( created, dto.LastModified );
        Assert.Equal( new[] { "a", "b" }, dto.Subtasks!.Select( s => s.Id ) );
    }
}
