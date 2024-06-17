using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

using SET.WebAPI.Helpers;

namespace SET.WebAPI;

public static class Program
{
    public static int Main(string[] args)
    {
        var kyivTimeZone = TimeZoneInfo.FindSystemTimeZoneById( id: "FLE Standard Time" );

        Log.Logger = new LoggerConfiguration()
            .Enrich.With( new TimeZoneEnricher( kyivTimeZone ) )
            .WriteTo.Console()
            .CreateBootstrapLogger();

        Log.Information( "Starting up!" );

        if (args is null || args.Length == 0)
        {
            Log.Information( "args parameter is null or empty" );
        }
        else
        {
            Log.Information( string.Join( separator: ", ", args ) );
        }

        try
        {
            IHostBuilder hostBuilder = CreateHostBuilder( args );
            Log.Information( "hostBuilder successfully created" );
            IHost host = hostBuilder.Build();
            Log.Information( "host successfully built" );
            host.Run();
            Log.Information( "Stoppend cleanly" );
            return 0;
        }
        catch(Exception ex)
        {
            Log.Fatal( ex, "An unhandled exception occurred during bootstrapping" );
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
                   .ConfigureWebHostDefaults( webBuilder =>
                   {
                       webBuilder.UseStartup<Startup>();
                   } );
    }
}
