using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace SET.WebAPI;

public static class Program
{
    public static int Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        Log.Information( "Starting up!" );

        if (args is null)
        {
            Log.Information( "args parameter is null" );
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
                   .UseSerilog((context, services, configuration) =>
                   {
                       configuration.
                           ReadFrom.Configuration( context.Configuration ).
                           ReadFrom.Services( services ).
                           Enrich.FromLogContext();
                   })
                   .ConfigureWebHostDefaults( webBuilder =>
                   {
                       webBuilder.UseStartup<Startup>();
                       webBuilder.UseUrls( "http://0.0.0.0:80" );
                   } );
    }
}
