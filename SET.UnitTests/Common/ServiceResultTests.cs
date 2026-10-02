using BusinessLogic;

namespace SET.UnitTests.Common;

public class ServiceResultTests
{
    [Fact]
    public void Success_has_no_error()
    {
        Assert.True( ServiceResult.Success.IsSuccess );
        Assert.Null( ServiceResult.Success.Error );
    }

    [Fact]
    public void BadRequest_and_NotFound_carry_status_and_body()
    {
        Assert.Equal( new ServiceError( 400, "Token" ), ServiceError.BadRequest( "Token" ) );
        Assert.Equal( new ServiceError( 404, "Token" ), ServiceError.NotFound( "Token" ) );
    }

    [Fact]
    public void Error_converts_to_failed_result()
    {
        ServiceResult result = ServiceError.BadRequest( "Token" );

        Assert.False( result.IsSuccess );
        Assert.Equal( 400, result.Error!.StatusCode );
    }

    [Fact]
    public void Value_converts_to_successful_generic_result()
    {
        ServiceResult<string> result = "value";

        Assert.True( result.IsSuccess );
        Assert.Equal( "value", result.Value );
    }

    [Fact]
    public void Error_converts_to_failed_generic_result_without_value()
    {
        ServiceResult<string> result = ServiceError.NotFound( "Missing" );

        Assert.False( result.IsSuccess );
        Assert.Null( result.Value );
        Assert.Equal( "Missing", result.Error!.Body );
    }
}
