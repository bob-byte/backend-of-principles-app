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
        connectionString = "Host=localhost;Database=SET;Port=5432;Username=postgres;Password=qwerty";
#endif
        optsBuilder.UseNpgsql( connectionString );
        return new AppDbContext();
    }
}