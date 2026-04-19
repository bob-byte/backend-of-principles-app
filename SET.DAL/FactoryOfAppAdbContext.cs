using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

using System;
namespace SET.DataAccess;

public class FactoryOfAppContext : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext( string[] args )
    {
        DbContextOptionsBuilder<AppDbContext> optsBuilder = new();
        string connectionString;
#if DEBUG
        connectionString = "Host=localhost;Database=SET;Port=5432;Username=postgres;Password=qwerty";
#else
        connectionString = "Host=principles_database;Port=5432;Username=postgres;Password=76193db1d01e34743d5c;Database=principles;";
#endif
        optsBuilder.UseNpgsql( connectionString );
        return new AppDbContext( optsBuilder.Options );
    }
}