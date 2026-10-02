using BusinessLogic;
using BusinessLogic.Models;
using SET.DataAccess;
using SET.UnitTests.TestSupport;

namespace SET.UnitTests.Services;

public class ClientLogServiceTests
{
    private static (ClientLogService service, AppDbContext db) CreateSut()
    {
        AppDbContext db = TestDb.Create();
        return (new ClientLogService( db, BusinessLogicMapper.Create() ), db);
    }

    private static SaveLogRequest Request( string logType = "Error" ) => new()
    {
        DeviceOs = "iOS 26",
        DeviceType = "Physical",
        DeviceManufacturer = "Apple",
        AppVersion = "2.0.0",
        LogType = logType,
        LogMessage = "Something failed",
        StackTrace = "at Foo()",
    };

    [Fact]
    public async Task WriteAsync_rejects_unknown_user()
    {
        (ClientLogService service, _) = CreateSut();

        TestData.AssertError( await service.WriteAsync( Request(), 99 ), 400, "UserIsNotFound" );
    }

    [Theory]
    [InlineData( null )]
    [InlineData( 0L )]
    public async Task WriteAsync_accepts_anonymous_logs( long? userId )
    {
        (ClientLogService service, _) = CreateSut();

        Assert.True( (await service.WriteAsync( Request(), userId )).IsSuccess );
    }

    [Theory]
    [InlineData( "Information" )]
    [InlineData( "warning" )]
    [InlineData( "not-a-level" )]
    public async Task WriteAsync_accepts_known_user_with_any_level( string logType )
    {
        (ClientLogService service, AppDbContext db) = CreateSut();
        await TestData.AddUserAsync( db, 1 );

        Assert.True( (await service.WriteAsync( Request( logType ), 1 )).IsSuccess );
    }
}
