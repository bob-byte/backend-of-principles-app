namespace BusinessLogic;

/// <summary>Writes client app logs to the server Serilog sinks.</summary>
public interface IClientLogService
{
    /// <param name="userId">Caller id from the access token, when the request had a valid one.</param>
    Task<ServiceResult> WriteAsync( SaveLogRequest saveLogRequest, long? userId );
}
