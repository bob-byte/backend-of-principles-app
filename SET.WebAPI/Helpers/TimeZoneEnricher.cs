using Serilog.Core;
using Serilog.Events;

namespace SET.WebAPI.Helpers;

public class TimeZoneEnricher : ILogEventEnricher
{
    private readonly TimeZoneInfo m_timeZone;

    public TimeZoneEnricher(TimeZoneInfo timeZone)
    {
        m_timeZone = timeZone;
    }

    public void Enrich( LogEvent logEvent, ILogEventPropertyFactory propertyFactory )
    {
        DateTime localDatetime = TimeZoneInfo.ConvertTimeFromUtc( logEvent.Timestamp.UtcDateTime, m_timeZone );
        logEvent.AddPropertyIfAbsent( propertyFactory.CreateProperty( name: "LocalTimestamp", value: localDatetime ) );
    }
}

