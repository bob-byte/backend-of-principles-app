using AutoMapper;

using Serilog.Events;

namespace BusinessLogic;

public class ClientLogService : IClientLogService
{
    private readonly AppDbContext m_dbContext;
    private readonly IMapper m_mapper;

    public ClientLogService( AppDbContext dbContext, IMapper mapper )
    {
        m_dbContext = dbContext;
        m_mapper = mapper;
    }

    public async Task<ServiceResult> WriteAsync( SaveLogRequest saveLogRequest, long? userId )
    {
        ServiceResult? result = null;
        ClientLog newClientLog = m_mapper.Map<ClientLog>( saveLogRequest );
        newClientLog.UserId = userId;

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
            User? user = await m_dbContext.Users.FindAsync( newClientLog.UserId ).DefaultConfigureAwait();
            if (user is null)
            {
                result = ServiceError.BadRequest( "UserIsNotFound" );
            }
            else
            {
                email = user.Email;
            }

            if (logEventLevel == LogEventLevel.Information && (email is "batsbohdan@gmail.com" or "bac.bogdan222@gmail.com"))
            {
                result = ServiceResult.Success;
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
                convertedLog =
                    $"{(string.IsNullOrWhiteSpace( email ) ? "Somebody" : email)} uses the app. Log message: {newClientLog.LogMessage}";
            }
            else
            {
                convertedLog = $"{nameof( SaveLogRequest.LogMessage )} = {saveLogRequest.LogMessage};{newLine}" +
                               (string.IsNullOrWhiteSpace( saveLogRequest.StackTrace )
                                   ? string.Empty
                                   : $"{nameof( SaveLogRequest.StackTrace )} = {saveLogRequest.StackTrace};{newLine}") +
                               (newClientLog.UserId is null or 0
                                   ? string.Empty
                                   : $"Email = {email};{newLine}") +
                               $"{nameof( SaveLogRequest.AppVersion )} = {saveLogRequest.AppVersion};{newLine}" +
                               $"{nameof( SaveLogRequest.DeviceOs )} = {saveLogRequest.DeviceOs}.";
            }

            Log.Write( logEventLevel, convertedLog );

            result = ServiceResult.Success;
        }

        return result;
    }
}
