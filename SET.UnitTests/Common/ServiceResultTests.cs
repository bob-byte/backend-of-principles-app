using BusinessLogic;

namespace SET.UnitTests.Common;

public class ServiceResultTests
{
    [Fact]
    public void Success_Default_HasNoError()
    {
        Assert.True( ServiceResult.Success.IsSuccess );
        Assert.Null( ServiceResult.Success.Error );
    }

    [Fact]
    public void BadRequestAndNotFound_Factory_CarryStatusAndBody()
    {
        Assert.Equal( new ServiceError( 400, "Token" ), ServiceError.BadRequest( "Token" ) );
        Assert.Equal( new ServiceError( 404, "Token" ), ServiceError.NotFound( "Token" ) );
    }

    [Fact]
    public void Error_ServiceError_ConvertsToFailedResult()
    {
        ServiceResult result = ServiceError.BadRequest( "Token" );

        Assert.False( result.IsSuccess );
        Assert.Equal( 400, result.Error!.StatusCode );
    }

    [Fact]
    public void Value_ImplicitConversion_ConvertsToSuccessfulGenericResult()
    {
        ServiceResult<string> result = "value";

        Assert.True( result.IsSuccess );
        Assert.Equal( "value", result.Value );
    }

    [Fact]
    public void Error_ServiceError_ConvertsToFailedGenericWithoutValue()
    {
        ServiceResult<string> result = ServiceError.NotFound( "Missing" );

        Assert.False( result.IsSuccess );
        Assert.Null( result.Value );
        Assert.Equal( "Missing", result.Error!.Body );
    }
}
