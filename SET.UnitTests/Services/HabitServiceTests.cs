using BusinessLogic;
using BusinessLogic.Models;
using Microsoft.EntityFrameworkCore;
using SET.DataAccess;
using SET.Shared.Models;
using SET.UnitTests.TestSupport;

namespace SET.UnitTests.Services;

public class HabitServiceTests
{
    private static (HabitService service, AppDbContext db, RecordingSyncPushService push) CreateSut( string? dbName = null )
    {
        AppDbContext db = TestDb.Create( dbName );
        RecordingSyncPushService push = new();
        var mapper = BusinessLogicMapper.Create();
        return (new HabitService( db, mapper, new ReminderService( db, mapper ), push ), db, push);
    }

    private static EditUserHabitDto NewHabitDto( string name = "Read" ) => new()
    {
        Name = name,
        ColorName = "#123456",
        Status = StatusOfHabit.InProgress,
        Frequency = new EditUserHabitDto.FrequencyDto { Type = FrequencyType.EveryDay, Repeats = 1, IntervalLengthInDays = 1 },
    };

    private static EditUserHabitDto ExistingHabitDto( UserHabit habit, string? name = null )
    {
        EditUserHabitDto dto = NewHabitDto( name ?? habit.Name );
        dto.Id = habit.Id;
        dto.ColorName = habit.ColorName;
        dto.Frequency.Id = habit.FrequencyId;
        return dto;
    }

    private static UserHabitReminderDto ReminderDto( params DayOfWeek[] days ) => new()
    {
        Title = "Reminder",
        Description = "Do it",
        Time = new TimeOnly( 9, 0 ),
        IsEnabled = true,
        DaysOfWeek = days.Select( d => new WeekDayDto { Type = d } ).ToArray(),
    };

    [Fact]
    public async Task GetInProgressAsync_ActiveHabits_ReturnsOrderedWithReminders()
    {
        (HabitService service, AppDbContext db, _) = CreateSut();
        UserHabit second = await TestData.AddHabitAsync( db, 1, "Second", h => h.Priority = 2 );
        await TestData.AddHabitAsync( db, 1, "First", h => h.Priority = 1 );
        await TestData.AddHabitAsync( db, 1, "Archived", h => h.IsArchived = true );
        await TestData.AddHabitAsync( db, 1, "Frozen", h => h.Status = StatusOfHabit.Frozen );
        await TestData.AddHabitAsync( db, 2, "Foreign" );
        await TestData.AddHabitReminderAsync( db, second.Id, "Ping", (DayOfWeek.Monday, 100) );

        List<UserHabitInProgressShortDto> habits = await service.GetInProgressAsync( 1 );

        Assert.Equal( new[] { "First", "Second" }, habits.Select( h => h.Name ) );
        Assert.Null( habits[0].Reminders );
        Assert.Equal( "Ping", Assert.Single( habits[1].Reminders ).Title );
    }

    [Fact]
    public async Task GetArchivedAsync_ArchivedHabits_ReturnsNewestFirst()
    {
        (HabitService service, AppDbContext db, _) = CreateSut();
        UserHabit older = await TestData.AddHabitAsync( db, 1, "Older", h => h.IsArchived = true );
        UserHabit newer = await TestData.AddHabitAsync( db, 1, "Newer", h => h.IsArchived = true );
        await TestData.AddHabitAsync( db, 1, "Active" );
        await TestData.AddHabitAsync( db, 2, "Foreign", h => h.IsArchived = true );

        List<ArchivedHabitResponse> habits = await service.GetArchivedAsync( 1 );

        Assert.Equal( new[] { newer.Id, older.Id }, habits.Select( h => h.Id ) );
    }

    [Fact]
    public async Task GetForEditAsync_MissingHabit_ReturnsBadRequest()
    {
        (HabitService service, _, _) = CreateSut();

        TestData.AssertError( await service.GetForEditAsync( 1, 5 ), 400, "UserHabit is not found" );
    }

    [Fact]
    public async Task GetForEditAsync_OwnedHabit_MapsWithReminder()
    {
        (HabitService service, AppDbContext db, _) = CreateSut();
        UserHabit habit = await TestData.AddHabitAsync( db, 1, "Read" );
        await TestData.AddHabitReminderAsync( db, habit.Id, "Ping", (DayOfWeek.Tuesday, 120) );

        ServiceResult<EditUserHabitDto> result = await service.GetForEditAsync( 1, habit.Id );

        Assert.Equal( "Read", result.Value!.Name );
        UserHabitReminderDto reminder = Assert.Single( result.Value.Reminders );
        Assert.Equal( 120, Assert.Single( reminder.DaysOfWeek ).UserNotificationRequestId );
    }

    [Fact]
    public async Task SetArchiveStatusAsync_OwnedHabit_TogglesArchiveAndReminders()
    {
        (HabitService service, AppDbContext db, RecordingSyncPushService push) = CreateSut();
        UserHabit habit = await TestData.AddHabitAsync( db, 1, "Read" );
        UserHabitReminder reminder = await TestData.AddHabitReminderAsync( db, habit.Id, "Ping" );

        await service.SetArchiveStatusAsync( 1, new HabitArchiveStatus { HabitId = habit.Id, IsArchived = true }, "dev" );

        Assert.True( habit.IsArchived );
        Assert.False( reminder.IsEnabled );
        Assert.Equal( habit.UpdatedAt, habit.ArchivingTime );

        await service.SetArchiveStatusAsync( 1, new HabitArchiveStatus { HabitId = habit.Id, IsArchived = false }, "dev" );

        Assert.False( habit.IsArchived );
        Assert.True( reminder.IsEnabled );
        Assert.Null( habit.ArchivingTime );
        Assert.Equal( 2, push.Requests.Count );
    }

    [Fact]
    public async Task GetForEditAsync_OtherUsersHabit_ReturnsBadRequest()
    {
        (HabitService service, AppDbContext db, _) = CreateSut();
        UserHabit foreign = await TestData.AddHabitAsync( db, 2, "Foreign" );

        TestData.AssertError( await service.GetForEditAsync( 1, foreign.Id ), 400, "UserHabit is not found" );
    }

    [Fact]
    public async Task SetArchiveStatusAsync_NullMissingOrForeign_ReturnsError()
    {
        (HabitService service, AppDbContext db, RecordingSyncPushService push) = CreateSut();
        UserHabit foreign = await TestData.AddHabitAsync( db, 2, "Foreign" );
        UserHabitReminder reminder = await TestData.AddHabitReminderAsync( db, foreign.Id, "Ping" );

        TestData.AssertError( await service.SetArchiveStatusAsync( 1, null!, null ), 400, "HabitArchiveStatusIsNull" );
        TestData.AssertError(
            await service.SetArchiveStatusAsync( 1, new HabitArchiveStatus { HabitId = 404, IsArchived = true }, null ),
            404,
            "HabitIsNotFoundWithId 404" );
        TestData.AssertError(
            await service.SetArchiveStatusAsync( 1, new HabitArchiveStatus { HabitId = foreign.Id, IsArchived = true }, null ),
            404,
            $"HabitIsNotFoundWithId {foreign.Id}" );

        Assert.False( foreign.IsArchived );
        Assert.True( reminder.IsEnabled );
        Assert.Empty( push.Requests );
    }

    [Fact]
    public async Task SaveAsync_OtherUsersHabitId_ReturnsNotFound()
    {
        string dbName = Guid.NewGuid().ToString();
        UserHabit foreign = await TestData.AddHabitAsync( TestDb.Create( dbName ), 2, "Foreign" );
        (HabitService service, _, RecordingSyncPushService push) = CreateSut( dbName );

        ServiceResult<HabitSavedResponse> result = await service.SaveAsync( 1, ExistingHabitDto( foreign, "Hijacked" ), null );

        TestData.AssertError( result, 404, $"HabitIsNotFoundWithId {foreign.Id}" );
        UserHabit stored = await TestDb.Create( dbName ).UserHabits.SingleAsync();
        Assert.Equal( (2L, "Foreign"), (stored.UserId, stored.Name) );
        Assert.Empty( push.Requests );
    }

    [Fact]
    public async Task SaveAsync_Update_KeepsOwnedFrequencyRow()
    {
        string dbName = Guid.NewGuid().ToString();
        AppDbContext seed = TestDb.Create( dbName );
        UserHabit mine = await TestData.AddHabitAsync( seed, 1, "Mine" );
        UserHabit foreign = await TestData.AddHabitAsync( seed, 2, "Foreign" );
        (HabitService service, _, _) = CreateSut( dbName );

        EditUserHabitDto dto = ExistingHabitDto( mine );
        dto.Frequency.Id = foreign.FrequencyId;
        dto.Frequency.Type = FrequencyType.SeveralTimesPerPeriod;

        ServiceResult<HabitSavedResponse> result = await service.SaveAsync( 1, dto, null );

        Assert.Equal( mine.FrequencyId, result.Value!.FrequencyId );
        AppDbContext check = TestDb.Create( dbName );
        Assert.Equal( FrequencyType.SeveralTimesPerPeriod, (await check.Frequencies.FindAsync( mine.FrequencyId ))!.Type );
        Assert.Equal( FrequencyType.EveryDay, (await check.Frequencies.FindAsync( foreign.FrequencyId ))!.Type );
    }

    [Fact]
    public async Task SaveAsync_NullHabitOrFrequency_ReturnsBadRequest()
    {
        (HabitService service, _, RecordingSyncPushService push) = CreateSut();

        TestData.AssertError( await service.SaveAsync( 1, null!, null ), 400, "HabitIsNull" );

        EditUserHabitDto noFrequency = NewHabitDto();
        noFrequency.Frequency = null!;
        TestData.AssertError( await service.SaveAsync( 1, noFrequency, null ), 400, "FrequencyIsNull" );
        Assert.Empty( push.Requests );
    }

    // New-habit saves are not covered: EF InMemory writes generated keys onto the entity on Add,
    // so AddOrUpdateAsync(habit.Frequency) turns into an Update. Npgsql keeps keys temporary.

    [Fact]
    public async Task SaveAsync_ExistingHabit_UpdatesFrequencyAndPriorities()
    {
        string dbName = Guid.NewGuid().ToString();
        AppDbContext seed = TestDb.Create( dbName );
        UserHabit habit = await TestData.AddHabitAsync( seed, 1, "Old", h => h.Priority = 1 );
        UserHabit other = await TestData.AddHabitAsync( seed, 1, "Other", h => h.Priority = 2 );
        (HabitService service, _, RecordingSyncPushService push) = CreateSut( dbName );

        EditUserHabitDto dto = ExistingHabitDto( habit, "New" );
        dto.Priority = 2;
        dto.Description = "desc";
        dto.Frequency.Type = FrequencyType.SeveralTimesPerPeriod;
        dto.PrioritizedHabits = new List<UserHabitWithPriority> { new() { Id = other.Id, Priority = 1 } };

        ServiceResult<HabitSavedResponse> result = await service.SaveAsync( 1, dto, "dev" );

        Assert.Equal( habit.Id, result.Value!.Id );
        Assert.Equal( habit.FrequencyId, result.Value.FrequencyId );
        AppDbContext check = TestDb.Create( dbName );
        UserHabit stored = await check.UserHabits.Include( h => h.Frequency ).SingleAsync( h => h.Id == habit.Id );
        Assert.Equal( "New", stored.Name );
        Assert.Equal( "desc", stored.Description );
        Assert.Equal( 2, stored.Priority );
        Assert.NotNull( stored.UpdatedAt );
        Assert.Equal( FrequencyType.SeveralTimesPerPeriod, stored.Frequency.Type );
        Assert.Equal( 1, (await check.UserHabits.SingleAsync( h => h.Id == other.Id )).Priority );
        Assert.Equal( "dev", Assert.Single( push.Requests ).OriginDeviceId );
    }

    [Theory]
    [InlineData( null, "#1C1C1C" )]
    [InlineData( "  ", "#1C1C1C" )]
    [InlineData( "#ABCDEF", "#ABCDEF" )]
    [InlineData( "#123456789ABCDEF", "#123456789" )]
    public async Task SaveAsync_InvalidColorName_SanitizesValue( string? colorName, string expected )
    {
        string dbName = Guid.NewGuid().ToString();
        UserHabit habit = await TestData.AddHabitAsync( TestDb.Create( dbName ), 1, "Read" );
        (HabitService service, _, _) = CreateSut( dbName );
        EditUserHabitDto dto = ExistingHabitDto( habit );
        dto.ColorName = colorName!;

        await service.SaveAsync( 1, dto, null );

        Assert.Equal( expected, (await TestDb.Create( dbName ).UserHabits.SingleAsync()).ColorName );
    }

    [Fact]
    public async Task SaveAsync_ForeignGoalId_DropsLinkKeepsOwn()
    {
        string dbName = Guid.NewGuid().ToString();
        AppDbContext seed = TestDb.Create( dbName );
        UserGoal mine = await TestData.AddGoalAsync( seed, 1, "Mine" );
        UserGoal theirs = await TestData.AddGoalAsync( seed, 2, "Theirs" );
        UserHabit own = await TestData.AddHabitAsync( seed, 1, "Own" );
        UserHabit foreign = await TestData.AddHabitAsync( seed, 1, "Foreign" );

        EditUserHabitDto ownDto = ExistingHabitDto( own );
        ownDto.Goal = new UserGoalDto { Id = mine.Id, Name = mine.Name };
        EditUserHabitDto foreignDto = ExistingHabitDto( foreign );
        foreignDto.Goal = new UserGoalDto { Id = theirs.Id, Name = theirs.Name };

        await CreateSut( dbName ).service.SaveAsync( 1, ownDto, null );
        await CreateSut( dbName ).service.SaveAsync( 1, foreignDto, null );

        AppDbContext check = TestDb.Create( dbName );
        Assert.Equal( mine.Id, (await check.UserHabits.FindAsync( own.Id ))!.GoalId );
        Assert.Null( (await check.UserHabits.FindAsync( foreign.Id ))!.GoalId );
    }

    [Fact]
    public async Task SaveAsync_NewReminders_AssignsNotificationRequestIds()
    {
        string dbName = Guid.NewGuid().ToString();
        UserHabit habit = await TestData.AddHabitAsync( TestDb.Create( dbName ), 1, "Read" );
        (HabitService service, _, _) = CreateSut( dbName );
        EditUserHabitDto dto = ExistingHabitDto( habit );
        dto.Reminders = new List<UserHabitReminderDto> { ReminderDto( DayOfWeek.Monday, DayOfWeek.Wednesday ) };

        ServiceResult<HabitSavedResponse> result = await service.SaveAsync( 1, dto, null );

        ReminderIds reminder = Assert.Single( result.Value!.ReminderIds );
        Assert.Equal( new[] { 100, 101 }, reminder.DaysOfWeek.Select( d => d.NotificationRequestId ).Order() );
        AppDbContext check = TestDb.Create( dbName );
        Assert.Equal( 101, (await check.TrackingOfUserNotificationRequests.SingleAsync()).MaxNotificationRequestId );
        Assert.Equal( "Reminder", (await check.UserHabitReminders.SingleAsync()).Title );
    }

    [Fact]
    public async Task ResetPrioritiesAsync_FewerThanTwoHabits_ReturnsBadRequest()
    {
        (HabitService service, _, _) = CreateSut();

        TestData.AssertError( await service.ResetPrioritiesAsync( 1, null! ), 400, "Habits with priorities are less than 2" );
        TestData.AssertError(
            await service.ResetPrioritiesAsync( 1, new List<UserHabitWithPriority> { new() { Id = 1, Priority = 1 } } ),
            400,
            "Habits with priorities are less than 2" );
    }

    [Fact]
    public async Task ResetPrioritiesAsync_ValidPayload_AppliesPriorities()
    {
        (HabitService service, AppDbContext db, _) = CreateSut();
        UserHabit a = await TestData.AddHabitAsync( db, 1, "A", h => h.Priority = 1 );
        UserHabit b = await TestData.AddHabitAsync( db, 1, "B", h => h.Priority = 2 );

        ServiceResult result = await service.ResetPrioritiesAsync( 1, new List<UserHabitWithPriority>
        {
            new() { Id = a.Id, Priority = 2 },
            new() { Id = b.Id, Priority = 1 },
        } );

        Assert.True( result.IsSuccess );
        Assert.Equal( 2, a.Priority );
        Assert.Equal( 1, b.Priority );
    }

    [Fact]
    public async Task ResetPrioritiesAsync_PartialPayload_SkipsMissingHabits()
    {
        (HabitService service, AppDbContext db, _) = CreateSut();
        UserHabit a = await TestData.AddHabitAsync( db, 1, "A", h => h.Priority = 1 );
        UserHabit b = await TestData.AddHabitAsync( db, 1, "B", h => h.Priority = 2 );
        UserHabit c = await TestData.AddHabitAsync( db, 1, "C", h => h.Priority = 3 );

        ServiceResult result = await service.ResetPrioritiesAsync( 1, new List<UserHabitWithPriority>
        {
            new() { Id = a.Id, Priority = 9 },
            new() { Id = b.Id, Priority = 8 },
        } );

        Assert.True( result.IsSuccess );
        Assert.Equal( 9, a.Priority );
        Assert.Equal( 8, b.Priority );
        Assert.Equal( 3, c.Priority );
    }

    [Fact]
    public async Task DeleteAsync_ZeroOrForeignId_ReturnsError()
    {
        (HabitService service, AppDbContext db, RecordingSyncPushService push) = CreateSut();
        UserHabit foreign = await TestData.AddHabitAsync( db, 2, "Foreign" );

        TestData.AssertError( await service.DeleteAsync( 1, 0, null ), 400, "HabitIdIsZero" );
        TestData.AssertError( await service.DeleteAsync( 1, foreign.Id, null ), 404, $"HabitIsNotFoundWithId {foreign.Id}" );
        Assert.Empty( push.Requests );
        Assert.Empty( db.SyncDeletions );
    }
}
