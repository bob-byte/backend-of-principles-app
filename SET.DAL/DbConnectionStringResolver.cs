using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SET.DataAccess;

internal static class DbConnectionStringResolver
{
    private const string DEFAULT_CONNECTION_NAME = "DefaultConnection";
    private const string HOSTED_CONNECTION_NAME = "Hostinger";
    private const string LOCAL_FALLBACK_CONNECTION_STRING = "Host=localhost;Database=SET;Port=5432;Username=postgres;Password=qwerty";

    public static string Resolve()
    {
        IConfigurationRoot configuration = BuildConfiguration();
        string environmentName = GetEnvironmentName();
        bool isDevelopment = environmentName.Equals( "Development", StringComparison.OrdinalIgnoreCase );
        string connectionName = isDevelopment ? DEFAULT_CONNECTION_NAME : HOSTED_CONNECTION_NAME;

        string? connectionString = configuration.GetConnectionString( connectionName );
        if (string.IsNullOrWhiteSpace( connectionString ) && !isDevelopment)
        {
            connectionString = configuration.GetConnectionString( DEFAULT_CONNECTION_NAME );
        }

        return string.IsNullOrWhiteSpace( connectionString ) ? LOCAL_FALLBACK_CONNECTION_STRING : connectionString;
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        string settingsDirectory = FindSettingsDirectory();
        string environmentName = GetEnvironmentName();

        return new ConfigurationBuilder()
            .SetBasePath( settingsDirectory )
            .AddJsonFile( "appsettings.json", optional: true, reloadOnChange: false )
            .AddJsonFile( $"appsettings.{environmentName}.json", optional: true, reloadOnChange: false )
            .AddEnvironmentVariables()
            .Build();
    }

    private static string FindSettingsDirectory()
    {
        IEnumerable<string> probeRoots = new[]
        {
            Directory.GetCurrentDirectory(),
            AppContext.BaseDirectory
        };

        string? settingsDirectory = probeRoots
            .Where( Directory.Exists )
            .Select( Path.GetFullPath )
            .SelectMany( EnumerateCandidateDirectories )
            .FirstOrDefault( directory => File.Exists( Path.Combine( directory, "appsettings.json" ) ) );

        return settingsDirectory ?? Directory.GetCurrentDirectory();
    }

    private static IEnumerable<string> EnumerateCandidateDirectories( string startDirectory )
    {
        DirectoryInfo? current = new( startDirectory );
        while (current != null)
        {
            yield return current.FullName;
            yield return Path.Combine( current.FullName, "SET.WebAPI" );
            yield return Path.Combine( current.FullName, "backend-of-principles-app", "SET.WebAPI" );

            current = current.Parent;
        }
    }

    private static string GetEnvironmentName()
    {
        return Environment.GetEnvironmentVariable( "ASPNETCORE_ENVIRONMENT" )
            ?? Environment.GetEnvironmentVariable( "DOTNET_ENVIRONMENT" )
            ?? "Development";
    }
}