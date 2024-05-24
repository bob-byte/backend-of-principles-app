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
            Log.Information( string.Join( ", ", args ) );
        }
        else
        {
            Log.Information( "args parameter is null" );
        }

        try
        {
            CreateHostBuilder( args ).Build().Run();
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
                   .ConfigureWebHostDefaults( webBuilder => webBuilder.UseStartup<Startup>() );
    }
}
