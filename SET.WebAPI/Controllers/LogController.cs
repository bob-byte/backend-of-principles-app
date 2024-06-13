using Serilog.Events;

using System;
namespace SET.WebAPI.Controllers;

[Route( template: "api/logs" )]
[ApiController]
public class LogController : BaseController
{
    public LogController( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        //do nothing
    }

    [HttpPost]
    public Task<IActionResult> LogToServerConsole( [FromBody] SaveLogRequest saveLogRequest )
    {
        return TryCatchAsync( async () =>
        {
            IActionResult? result = null;
            ClientLog newClientLog = Mapper.Map<ClientLog>( saveLogRequest );

            string email = string.Empty;
            if (newClientLog.UserId > 0)
            {
                User user = await DbContext.Users.FindAsync( newClientLog.UserId ).DefaultConfigureAwait();
                if (user is null)
                {
                    result = BadRequest( "UserIsNotFound" );
                }
                else
                {
                    email = user.Email;
                }
            }
            else
            {
                newClientLog.UserId = null;
            }

            if(result is null)
            {
                try
                {
                    await DbContext.ClientLogs.AddAsync( newClientLog ).DefaultConfigureAwait();
                    await DbContext.SaveChangesAsync().DefaultConfigureAwait();

                    string newLine = Environment.NewLine;
                    string convertedLog = $"{newLine}!!!ADDED NEW CLIENT LOG INTO DATABASE!!!{newLine}Its short description:{newLine}" +
                        $"{nameof( SaveLogRequest.LogMessage )} = {saveLogRequest.LogMessage};{newLine}" +
                        (string.IsNullOrWhiteSpace( saveLogRequest.StackTrace )
                            ? string.Empty
                            : $"{nameof( SaveLogRequest.StackTrace )} = {saveLogRequest.StackTrace};{newLine}") +
                        (saveLogRequest.UserId is 0
                            ? string.Empty
                            : $"User Email = {email};{newLine}") +
                        $"{nameof( SaveLogRequest.AppVersion )} = {saveLogRequest.AppVersion};{newLine}" +
                        $"{nameof( SaveLogRequest.DeviceOs )} = {saveLogRequest.DeviceOs}.";

                    bool isParsedLogEventLevel = Enum.TryParse( saveLogRequest.LogType, ignoreCase: true, out LogEventLevel logEventLevel );
                    if (!isParsedLogEventLevel)
                    {
                        logEventLevel = LogEventLevel.Error;
                    }

                    Log.Write( logEventLevel, convertedLog );

                    result = Ok();
                }
                catch (Exception ex)
                {
                    string newLine = Environment.NewLine;
                    string convertedLog = $"{newLine}!!!CANNOT ADD NEW CLIENT LOG INTO DATABASE!!!{newLine}Its full description:{newLine}" +
                        $"{nameof( SaveLogRequest.LogMessage )} = {saveLogRequest.LogMessage};{newLine}" +
                        (string.IsNullOrWhiteSpace( saveLogRequest.StackTrace )
                            ? string.Empty
                            : $"{nameof( SaveLogRequest.StackTrace )} = {saveLogRequest.StackTrace};{newLine}") +
                        (saveLogRequest.UserId is 0
                            ? string.Empty
                            : $"User Email = {email};{newLine}") +
                        $"{nameof( SaveLogRequest.AppVersion )} = {saveLogRequest.AppVersion};{newLine}" +
                        $"{nameof( SaveLogRequest.DeviceOs )} = {saveLogRequest.DeviceOs};{newLine}" +
                        $"{nameof( SaveLogRequest.DeviceModelName )} = {saveLogRequest.DeviceModelName};{newLine}" +
                        $"{nameof( SaveLogRequest.DeviceManufacturer )} = {saveLogRequest.DeviceManufacturer};{newLine}" +
                        $"{nameof( SaveLogRequest.DeviceType )} = {saveLogRequest.DeviceType}.";

                    Log.Error( ex, convertedLog );

                    throw;
                }
            }

            return result;
        } );
    }
}
