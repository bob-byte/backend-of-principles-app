using Microsoft.AspNetCore.Authentication;

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

            try
            {
                string accessToken = await HttpContext.GetTokenAsync( tokenName: "access_token" ).DefaultConfigureAwait();
                newClientLog.UserId = JwtTokenService.GetUserIdFromJwt( accessToken );
            }
            catch
            {
                newClientLog.UserId = null;
            }

            bool isParsedLogEventLevel = Enum.TryParse( saveLogRequest.LogType, ignoreCase: true,
                out LogEventLevel logEventLevel );
            if (!isParsedLogEventLevel)
            {
                Log.Error( "Cannot parse log event level." );
                logEventLevel = LogEventLevel.Error;
            }

            string email = string.Empty;
            if (newClientLog.UserId > 0)
            {
                User user = await DbContext.Users.FindAsync( newClientLog.UserId ).DefaultConfigureAwait();
                if (user is null)
                {
                    result = BadRequest( error: "UserIsNotFound" );
                }
                else
                {
                    email = user.Email;
                }

                if (logEventLevel == LogEventLevel.Information && (email is "batsbohdan@gmail.com" or "bac.bogdan222@gmail.com"))
                {
                    result = Ok();
                }
            }
            else
            {
                newClientLog.UserId = null;
            }

            if (result is null)
            {
                string newLine = Environment.NewLine;
                string convertedLog;

                if (logEventLevel == LogEventLevel.Information)
                {
                    convertedLog = $"{(string.IsNullOrWhiteSpace( email ) ? "Somebody" : email)} uses the app. Log message: {newClientLog.LogMessage}";
                }
                else
                {
                    convertedLog = $"{nameof( SaveLogRequest.LogMessage )} = {saveLogRequest.LogMessage};{newLine}" +
                                   (string.IsNullOrWhiteSpace( saveLogRequest.StackTrace )
                                       ? string.Empty
                                       : $"{nameof( SaveLogRequest.StackTrace )} = {saveLogRequest.StackTrace};{newLine}") +
                                   (newClientLog.UserId is null or 0
                                       ? string.Empty
                                       : $"User email = {email};{newLine}") +
                                   $"{nameof( SaveLogRequest.AppVersion )} = {saveLogRequest.AppVersion};{newLine}" +
                                   $"{nameof( SaveLogRequest.DeviceOs )} = {saveLogRequest.DeviceOs};{newLine}" +
                                   $"{nameof( SaveLogRequest.DeviceModelName )} = {saveLogRequest.DeviceModelName};{newLine}" +
                                   $"{nameof( SaveLogRequest.DeviceManufacturer )} = {saveLogRequest.DeviceManufacturer};{newLine}" +
                                   $"{nameof( SaveLogRequest.DeviceType )} = {saveLogRequest.DeviceType}.";
                }

                try
                {
                    if (logEventLevel != LogEventLevel.Information)
                    {
                        await DbContext.ClientLogs.AddAsync( newClientLog ).DefaultConfigureAwait();
                        await DbContext.SaveChangesAsync().DefaultConfigureAwait();
                    }

                    convertedLog = logEventLevel == LogEventLevel.Information ?
                        convertedLog :
                        $"{newLine}!!!ADDED NEW CLIENT LOG INTO DATABASE!!!{newLine}Its short description:{newLine}{convertedLog}";
                    Log.Write( logEventLevel, convertedLog );

                    result = Ok();
                }
                catch (Exception ex)
                {
                    convertedLog =
                        $"{newLine}!!!CANNOT ADD NEW CLIENT LOG INTO DATABASE!!!{newLine}Its full description:{newLine}{convertedLog}";
                    Log.Error( ex, convertedLog );

                    throw;
                }
            }

            return result;
        } );
    }
}
