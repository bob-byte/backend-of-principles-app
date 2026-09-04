using BusinessLogic;
using Microsoft.EntityFrameworkCore;
using SET.DataAccess;
using SET.Shared.Models;

namespace SET.UnitTests.Services;

public class ReminderServiceTests
{
    private static (ReminderService service, AppDbContext db) CreateSut()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new AppDbContext(options);
        return (new ReminderService(db), db);
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
}
