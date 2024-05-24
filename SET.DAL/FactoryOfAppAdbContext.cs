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
        connectionString = "postgres://postgres:76193db1d01e34743d5c@principles_database:5432/principles";
#endif
        optsBuilder.UseNpgsql( connectionString );
        return new AppDbContext();
    }
}