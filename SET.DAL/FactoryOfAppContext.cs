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
        connectionString = "Server=localhost;Database=SET;User=sa;Password=76FE5bs6rG;TrustServerCertificate=True;Connect Timeout=30;Encrypt=True";
#else
        connectionString = "Server=habitsmentorsetdbserver.database.windows.net;Initial Catalog=SET;Persist Security Info=False;User ID=habitsmentorset;Password=#1927Bodya;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
#endif
        optsBuilder.UseSqlServer( connectionString );
        return new AppDbContext();
    }
}

