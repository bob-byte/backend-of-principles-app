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
        connectionString = "Server=habitsmentorsetdbserver.database.windows.net;Initial Catalog=SET;Persist Security Info=False;User ID=habitsmentorset;Password=#1927Bodya;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
#endif
        optsBuilder.UseNpgsql( connectionString );
        return new AppDbContext();
    }
}