using BusinessLogic;
using SET.DataAccess;
using SET.Shared.Models;
using SET.UnitTests.TestSupport;

namespace SET.UnitTests.Services;

public class ProfileServiceTests
{
    private static (ProfileService service, AppDbContext db) CreateSut()
    {
        AppDbContext db = TestDb.Create();
        return (new ProfileService( db, BusinessLogicMapper.Create() ), db);
    }

    [Fact]
    public async Task GetProfile_UserWithoutUpdatedAt_FallsBackToCreatedAt()
    {
        (ProfileService service, AppDbContext db) = CreateSut();
        User user = await TestData.AddUserAsync( db, 4, "me@example.com", u =>
        {
            u.MainSlogan = "Slogan";
            u.Mission = "Mission";
            u.HasSeenRoadGuide = true;
        } );

        BusinessLogic.Models.Profile profile = service.GetProfile( user );

        Assert.Equal( 4, profile.Id );
        Assert.Equal( "me@example.com", profile.Email );
        Assert.Equal( "Slogan", profile.MainSlogan );
        Assert.Equal( "Mission", profile.Mission );
        Assert.Equal( Gender.Woman, profile.Gender );
        Assert.True( profile.HasSeenRoadGuide );
        Assert.Equal( user.CreatedAt, profile.LastModified );

        DateTime updated = new( 2026, 9, 1, 0, 0, 0, DateTimeKind.Utc );
        user.UpdatedAt = updated;
        Assert.Equal( updated, service.GetProfile( user ).LastModified );
    }

    [Theory]
    [InlineData( null )]
    [InlineData( "" )]
    [InlineData( "  " )]
    public async Task SaveNameAsync_BlankName_ReturnsBadRequest( string? name )
    {
        (ProfileService service, AppDbContext db) = CreateSut();
        User user = await TestData.AddUserAsync( db, 1 );

        TestData.AssertError( await service.SaveNameAsync( user, name! ), 400, "UserNameIsNullOrWhiteSpace" );
        Assert.Null( user.UpdatedAt );
    }

    [Fact]
    public async Task SaveNameAsync_ValidName_SavesAndBumpsUpdatedAt()
    {
        (ProfileService service, AppDbContext db) = CreateSut();
        User user = await TestData.AddUserAsync( db, 1 );

        ServiceResult result = await service.SaveNameAsync( user, "Alice" );

        Assert.True( result.IsSuccess );
        Assert.Equal( "Alice", (await db.Users.FindAsync( 1L ))!.Name );
        Assert.NotNull( user.UpdatedAt );
    }

    [Fact]
    public async Task SaveGenderAsync_UndefinedValue_ReturnsBadRequest()
    {
        (ProfileService service, AppDbContext db) = CreateSut();
        User user = await TestData.AddUserAsync( db, 1 );

        TestData.AssertError( await service.SaveGenderAsync( user, (Gender)42 ), 400, "GenderIsInvalid" );

        Assert.True( (await service.SaveGenderAsync( user, Gender.Man )).IsSuccess );
        Assert.Equal( Gender.Man, user.Gender );
    }

    [Fact]
    public async Task SaveMainSloganAndHasSeenRoadGuide_ValidValues_Persist()
    {
        (ProfileService service, AppDbContext db) = CreateSut();
        User user = await TestData.AddUserAsync( db, 1 );

        await service.SaveMainSloganAsync( user, "Keep going" );
        await service.SaveHasSeenRoadGuideAsync( user, true );

        User stored = (await db.Users.FindAsync( 1L ))!;
        Assert.Equal( "Keep going", stored.MainSlogan );
        Assert.True( stored.HasSeenRoadGuide );
        Assert.NotNull( stored.UpdatedAt );
    }

    [Fact]
    public async Task SaveMissionAsync_OldMissionOnReminders_RenamesThem()
    {
        (ProfileService service, AppDbContext db) = CreateSut();
        User user = await TestData.AddUserAsync( db, 1, configure: u => u.Mission = "Old mission" );
        await TestData.AddUserAsync( db, 2 );
        UserHabit habit = await TestData.AddHabitAsync( db, 1, "Habit" );
        UserHabit foreignHabit = await TestData.AddHabitAsync( db, 2, "Foreign" );
        UserHabitReminder habitReminder = await TestData.AddHabitReminderAsync( db, habit.Id, "Old mission" );
        UserHabitReminder foreignReminder = await TestData.AddHabitReminderAsync( db, foreignHabit.Id, "Old mission" );
        UserReminder report = await TestData.AddUserReminderAsync( db, 1, 1, title: "Old mission", description: "Old mission" );

        await service.SaveMissionAsync( user, "New mission" );

        Assert.Equal( "New mission", user.Mission );
        Assert.Equal( "New mission", habitReminder.Title );
        Assert.Equal( "Old mission", foreignReminder.Title );
        Assert.Equal( ("New mission", "New mission"), (report.Title, report.Description) );
        Assert.NotNull( user.UpdatedAt );
    }

    [Fact]
    public async Task SaveMissionAsync_MatchingReportFields_RenamesOnlyMatches()
    {
        (ProfileService service, AppDbContext db) = CreateSut();
        User user = await TestData.AddUserAsync( db, 1, configure: u => u.Mission = "Old mission" );
        UserReminder report = await TestData.AddUserReminderAsync( db, 1, 1, title: "Daily", description: "Old mission" );

        await service.SaveMissionAsync( user, "New mission" );

        Assert.Equal( ("Daily", "New mission"), (report.Title, report.Description) );
    }

    [Fact]
    public async Task SaveMissionAsync_NoPreviousMission_OnlySetsMission()
    {
        (ProfileService service, AppDbContext db) = CreateSut();
        User user = await TestData.AddUserAsync( db, 1 );
        UserHabit habit = await TestData.AddHabitAsync( db, 1, "Habit" );
        UserHabitReminder reminder = await TestData.AddHabitReminderAsync( db, habit.Id, "Title" );

        await service.SaveMissionAsync( user, "Mission" );

        Assert.Equal( "Mission", user.Mission );
        Assert.Equal( "Title", reminder.Title );
    }

    [Fact]
    public async Task SaveLastAppOpenAsync_NullExisting_PersistsUtcDateOnly()
    {
        (ProfileService service, AppDbContext db) = CreateSut();
        User user = await TestData.AddUserAsync( db, 1 );

        await service.SaveLastAppOpenAsync( user, new DateTime( 2026, 10, 5, 15, 30, 0, DateTimeKind.Local ) );

        User stored = (await db.Users.FindAsync( 1L ))!;
        Assert.Equal( new DateTime( 2026, 10, 5, 0, 0, 0, DateTimeKind.Utc ), stored.LastAppOpen );
        Assert.NotNull( stored.UpdatedAt );
        Assert.Equal( stored.LastAppOpen, service.GetProfile( user ).LastAppOpen );
    }

    [Fact]
    public async Task SaveLastAppOpenAsync_OlderOrSameDay_DoesNotUpdate()
    {
        (ProfileService service, AppDbContext db) = CreateSut();
        User user = await TestData.AddUserAsync( db, 1, configure: u =>
        {
            u.LastAppOpen = new DateTime( 2026, 10, 5, 0, 0, 0, DateTimeKind.Utc );
            u.UpdatedAt = new DateTime( 2026, 10, 5, 12, 0, 0, DateTimeKind.Utc );
        } );
        DateTime? previousUpdatedAt = user.UpdatedAt;

        await service.SaveLastAppOpenAsync( user, new DateTime( 2026, 10, 4, 0, 0, 0, DateTimeKind.Utc ) );
        await service.SaveLastAppOpenAsync( user, new DateTime( 2026, 10, 5, 0, 0, 0, DateTimeKind.Utc ) );

        User stored = (await db.Users.FindAsync( 1L ))!;
        Assert.Equal( new DateTime( 2026, 10, 5, 0, 0, 0, DateTimeKind.Utc ), stored.LastAppOpen );
        Assert.Equal( previousUpdatedAt, stored.UpdatedAt );
    }

    [Fact]
    public async Task SaveLastAppOpenAsync_NewerDay_ReplacesAndBumpsUpdatedAt()
    {
        (ProfileService service, AppDbContext db) = CreateSut();
        User user = await TestData.AddUserAsync( db, 1, configure: u =>
        {
            u.LastAppOpen = new DateTime( 2026, 10, 4, 0, 0, 0, DateTimeKind.Utc );
            u.UpdatedAt = new DateTime( 2026, 10, 4, 12, 0, 0, DateTimeKind.Utc );
        } );

        await service.SaveLastAppOpenAsync( user, new DateTime( 2026, 10, 5, 0, 0, 0, DateTimeKind.Utc ) );

        User stored = (await db.Users.FindAsync( 1L ))!;
        Assert.Equal( new DateTime( 2026, 10, 5, 0, 0, 0, DateTimeKind.Utc ), stored.LastAppOpen );
        Assert.True( stored.UpdatedAt > new DateTime( 2026, 10, 4, 12, 0, 0, DateTimeKind.Utc ) );
    }
}
