using Serilog.Events;

using System;
namespace SET.WebAPI.Controllers;

[Route( template: "api/logs" )]
[ApiController]
[Authorize( AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme )]
public class LogController : BaseController
{
    public LogController( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        //do nothing
    }

    [HttpPut( template: "{userId}" )]
    public Task<IActionResult> LogToServerConsole( long userId, [FromBody] SaveLogRequest saveLogRequest )
    {
        return TryCatchAsync( userId, ( User user ) =>
        {
            bool isParsedLogEventLevel = Enum.TryParse( saveLogRequest.LogType, ignoreCase: true, out LogEventLevel logEventLevel );
            if (!isParsedLogEventLevel)
            {
                logEventLevel = LogEventLevel.Error;
            }

            string newLine = Environment.NewLine;
            string convertedLog =
                $"{newLine}{nameof( SaveLogRequest.LogMessage )} = {saveLogRequest.LogMessage};{newLine}" +
                $"{nameof( SaveLogRequest.StackTrace )} = {saveLogRequest.StackTrace};{newLine}" +
                $"UserId = {userId};{newLine}" +
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
