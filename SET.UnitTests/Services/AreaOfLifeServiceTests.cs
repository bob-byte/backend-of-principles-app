using BusinessLogic;
using BusinessLogic.Models;
using SET.DataAccess;
using SET.UnitTests.TestSupport;

namespace SET.UnitTests.Services;

public class AreaOfLifeServiceTests
{
    [Fact]
    public async Task GetAllAsync_returns_only_the_users_areas()
    {
        AppDbContext db = TestDb.Create();
        await TestData.AddAreaOfLifeAsync( db, 1, "Health" );
        await TestData.AddAreaOfLifeAsync( db, 1, "Work" );
        await TestData.AddAreaOfLifeAsync( db, 2, "Foreign" );
        AreaOfLifeService service = new( db, BusinessLogicMapper.Create() );

        List<UserAreaOfLifeDto> areas = await service.GetAllAsync( 1 );

        Assert.Equal( new[] { "Health", "Work" }, areas.Select( a => a.Name ).Order() );
    }
}
