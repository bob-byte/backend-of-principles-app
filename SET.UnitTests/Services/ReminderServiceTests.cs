using BusinessLogic;
using BusinessLogic.Models;
using Microsoft.EntityFrameworkCore;
using SET.DataAccess;
using SET.Shared.Models;
using SET.UnitTests.TestSupport;

namespace SET.UnitTests.Services;

public class ReminderServiceTests
{
    private static (ReminderService service, AppDbContext db) CreateSut()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new AppDbContext(options);
        return (new ReminderService(db, BusinessLogicMapper.Create()), db);
    }

    [Fact]
    public async Task GetNotificationTrackingAsync_creates_default_tracking_when_missing()
    {
        (ReminderService service, AppDbContext db) = CreateSut();
        db.Users.Add(new User
        {
            Id = 7,
            Email = "ada@example.com",
            Name = "Ada",
            Gender = Gender.Woman,
            CreatedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();

        TrackingOfUserNotificationRequests tracking =
            await service.GetNotificationTrackingAsync(7);

        Assert.Equal(7, tracking.UserId);
        Assert.Equal(99, tracking.MaxNotificationRequestId);
        Assert.Equal(1, await db.TrackingOfUserNotificationRequests.CountAsync());
    }

    [Fact]
    public async Task GetNotificationTrackingAsync_returns_existing_row()
    {
        (ReminderService service, AppDbContext db) = CreateSut();
        db.Users.Add(new User
        {
            Id = 7,
            Email = "ada@example.com",
            Name = "Ada",
            Gender = Gender.Woman,
            CreatedAt = DateTime.UtcNow,
        });
        db.TrackingOfUserNotificationRequests.Add(new TrackingOfUserNotificationRequests
        {
            UserId = 7,
            MaxNotificationRequestId = 42,
        });
        await db.SaveChangesAsync();

        TrackingOfUserNotificationRequests tracking =
            await service.GetNotificationTrackingAsync(7);

        Assert.Equal(42, tracking.MaxNotificationRequestId);
        Assert.Equal(1, await db.TrackingOfUserNotificationRequests.CountAsync());
    }

    [Fact]
    public async Task GetHabitsReportReminderAsync_returns_empty_dto_when_missing()
    {
        (ReminderService service, AppDbContext db) = CreateSut();
        await TestData.AddUserAsync(db, 7);
        await TestData.AddUserReminderAsync(db, 7, notificationRequestId: 5);

        UserReminderDto dto = await service.GetHabitsReportReminderAsync(7);

        Assert.Equal(0, dto.Id);
        Assert.Null(dto.Title);
    }

    [Fact]
    public async Task GetHabitsReportReminderAsync_returns_notification_one()
    {
        (ReminderService service, AppDbContext db) = CreateSut();
        await TestData.AddUserAsync(db, 7);
        UserReminder report = await TestData.AddUserReminderAsync(db, 7, notificationRequestId: 1, title: "Daily");

        UserReminderDto dto = await service.GetHabitsReportReminderAsync(7);

        Assert.Equal(report.Id, dto.Id);
        Assert.Equal("Daily", dto.Title);
        Assert.Equal(1, dto.UserNotificationRequestId);
    }

    [Fact]
    public async Task GetAllRemindersAsync_returns_only_users_general_and_habit_reminders_with_days()
    {
        (ReminderService service, AppDbContext db) = CreateSut();
        await TestData.AddUserAsync(db, 7);
        await TestData.AddUserAsync(db, 8);
        await TestData.AddUserReminderAsync(db, 7, 1);
        await TestData.AddUserReminderAsync(db, 8, 1);
        UserHabit mine = await TestData.AddHabitAsync(db, 7, "Mine");
        UserHabit theirs = await TestData.AddHabitAsync(db, 8, "Theirs");
        await TestData.AddHabitReminderAsync(db, mine.Id, "Mine", (DayOfWeek.Monday, 100), (DayOfWeek.Friday, 101));
        await TestData.AddHabitReminderAsync(db, theirs.Id, "Theirs", (DayOfWeek.Monday, 200));

        AllRemindersResponse result = await service.GetAllRemindersAsync(7);

        Assert.Single(result.GeneralReminders);
        UserHabitReminderDto habitReminder = Assert.Single(result.UserHabitReminders);
        Assert.Equal("Mine", habitReminder.Title);
        Assert.Equal(new[] { 100, 101 }, habitReminder.DaysOfWeek.Select(d => d.UserNotificationRequestId).OrderBy(i => i));
    }

    [Fact]
    public async Task SaveHabitsReportReminderAsync_rejects_null()
    {
        (ReminderService service, AppDbContext db) = CreateSut();
        User user = await TestData.AddUserAsync(db, 7);

        ServiceResult<SavedReminderResponse> result = await service.SaveHabitsReportReminderAsync(user, null!);

        TestData.AssertError(result, 400, "HabitsReportReminderIsNullInSaveReminderEndpoint");
    }

    [Fact]
    public async Task SaveHabitsReportReminderAsync_forces_notification_one_and_returns_ids()
    {
        (ReminderService service, AppDbContext db) = CreateSut();
        User user = await TestData.AddUserAsync(db, 7);

        ServiceResult<SavedReminderResponse> result = await service.SaveHabitsReportReminderAsync(user, new UserReminderDto
        {
            Title = "Report",
            Description = "Check in",
            Time = new TimeOnly(21, 30),
            IsEnabled = true,
            UserNotificationRequestId = 55,
        });

        UserReminder stored = await db.UserReminders.SingleAsync();
        Assert.Equal(1, stored.UserNotificationRequestId);
        Assert.Equal(7, stored.UserId);
        Assert.Equal(stored.Id, result.Value!.Id);
        Assert.Equal(1, result.Value.UserNotificationRequestId);
    }

    [Fact]
    public async Task SaveHabitsReportReminderAsync_updates_existing_row_in_place()
    {
        (ReminderService service, AppDbContext db) = CreateSut();
        User user = await TestData.AddUserAsync(db, 7);
        UserReminder existing = await TestData.AddUserReminderAsync(db, 7, 1, title: "Old");

        ServiceResult<SavedReminderResponse> result = await service.SaveHabitsReportReminderAsync(user, new UserReminderDto
        {
            Title = "New",
            Description = "Desc",
            Time = new TimeOnly(20, 0),
            IsEnabled = false,
        });

        UserReminder stored = await db.UserReminders.SingleAsync();
        Assert.Equal(existing.Id, stored.Id);
        Assert.Equal(existing.Id, result.Value!.Id);
        Assert.Equal(("New", new TimeOnly(20, 0), false), (stored.Title, stored.Time, stored.IsEnabled));
    }

    [Fact]
    public async Task SaveHabitsReportReminderAsync_ignores_client_id_of_other_users_reminder()
    {
        (ReminderService service, AppDbContext db) = CreateSut();
        User user = await TestData.AddUserAsync(db, 7);
        await TestData.AddUserAsync(db, 8);
        UserReminder foreign = await TestData.AddUserReminderAsync(db, 8, 1, title: "Theirs");

        ServiceResult<SavedReminderResponse> result = await service.SaveHabitsReportReminderAsync(user, new UserReminderDto
        {
            Id = foreign.Id,
            Title = "Mine",
            Description = "Desc",
            Time = new TimeOnly(21, 0),
            IsEnabled = true,
        });

        Assert.NotEqual(foreign.Id, result.Value!.Id);
        Assert.Equal(("Theirs", 8L), (foreign.Title, foreign.UserId));
        Assert.Equal("Mine", (await db.UserReminders.SingleAsync(r => r.UserId == 7)).Title);
    }
}
