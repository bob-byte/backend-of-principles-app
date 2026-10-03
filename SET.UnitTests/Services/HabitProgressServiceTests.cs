using BusinessLogic;
using BusinessLogic.Models;
using SET.DataAccess;
using SET.Shared.Models;
using SET.UnitTests.TestSupport;

namespace SET.UnitTests.Services;

/// <summary>
/// Validation only: the success path uses <c>ExecuteUpdateAsync</c>, which EF InMemory does not support.
/// </summary>
public class HabitProgressServiceTests
{
    private static (HabitProgressService service, AppDbContext db) CreateSut()
    {
        AppDbContext db = TestDb.Create();
        return (new HabitProgressService( db, BusinessLogicMapper.Create() ), db);
    }

    [Fact]
    public async Task UpdateProgressAsync_NullProgress_ReturnsBadRequest()
    {
        (HabitProgressService service, _) = CreateSut();

        TestData.AssertError( await service.UpdateProgressAsync( 1, null! ), 400, "ProgressIsNull" );
    }

    [Fact]
    public async Task UpdateProgressAsync_DefaultDate_ReturnsBadRequest()
    {
        (HabitProgressService service, _) = CreateSut();

        ServiceResult<ProgressSavedResponse> result =
            await service.UpdateProgressAsync( 1, new UpdateProgressDto { HabitId = 3 } );

        TestData.AssertError( result, 400, $"Date is {default( DateOnly )}" );
    }

    [Fact]
    public async Task UpdateProgressAsync_MissingHabitId_ReturnsBadRequest()
    {
        (HabitProgressService service, _) = CreateSut();

        ServiceResult<ProgressSavedResponse> result =
            await service.UpdateProgressAsync( 1, new UpdateProgressDto { Date = new DateOnly( 2026, 10, 1 ) } );

        TestData.AssertError( result, 400, "HabitId of $UpdateProgressDto is not set" );
    }

    [Fact]
    public async Task UpdateProgressAsync_OtherUsersHabit_ReturnsBadRequest()
    {
        (HabitProgressService service, AppDbContext db) = CreateSut();
        UserHabit foreign = await TestData.AddHabitAsync( db, 2, "Foreign" );

        ServiceResult<ProgressSavedResponse> result = await service.UpdateProgressAsync( 1, new UpdateProgressDto
        {
            HabitId = foreign.Id,
            Date = new DateOnly( 2026, 10, 1 ),
            Value = 1,
        } );

        TestData.AssertError( result, 400, $"HabitIsNotFoundWithId {foreign.Id}" );
        Assert.Empty( db.ProgressesOfHabits );
    }
}
