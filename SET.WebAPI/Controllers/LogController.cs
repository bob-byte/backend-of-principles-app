using Serilog.Events;

using System;
namespace SET.WebAPI.Controllers;

[Route( template: "api/log" )]
[ApiController]
public class LogController : BaseController
{
    public LogController( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        //do nothing
    }

    [HttpPut]
    public Task<IActionResult> LogToServerConsole( [FromBody] SaveLogRequest saveLogRequest )
    {
        return TryCatchAsync( () =>
        {
            bool isParsedLogEventLevel = Enum.TryParse( saveLogRequest.LogType, ignoreCase: true, out LogEventLevel logEventLevel );
            if (!isParsedLogEventLevel)
            {
                logEventLevel = LogEventLevel.Error;
            }

            string newLine = Environment.NewLine;
            string convertedLog = $"Client log info:{newLine}" +
                $"{nameof( SaveLogRequest.LogMessage )} = {saveLogRequest.LogMessage};{newLine}" +
                (string.IsNullOrWhiteSpace( saveLogRequest.StackTrace )
                    ? string.Empty
                    : $"{nameof( SaveLogRequest.StackTrace )} = {saveLogRequest.StackTrace};{newLine}") +
                (saveLogRequest.UserId is null
                    ? ""
                    : $"{nameof( SaveLogRequest.UserId )} = {saveLogRequest.UserId};{newLine}") +
                $"{nameof( SaveLogRequest.AppVersion )} = {saveLogRequest.AppVersion};{newLine}" +
                $"{nameof( SaveLogRequest.DeviceOs )} = {saveLogRequest.DeviceOs};{newLine}" +
                $"{nameof( SaveLogRequest.DeviceModelName )} = {saveLogRequest.DeviceModelName};{newLine}" +
                $"{nameof( SaveLogRequest.DeviceManufacturer )} = {saveLogRequest.DeviceManufacturer};{newLine}" +
                $"{nameof( SaveLogRequest.DeviceType )} = {saveLogRequest.DeviceType}.";

            Log.Write( logEventLevel, convertedLog );

            IActionResult result = Ok();
            return Task.FromResult( result );
        } );
    }
}
