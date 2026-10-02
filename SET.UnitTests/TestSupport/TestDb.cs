using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SET.DataAccess;

namespace SET.UnitTests.TestSupport;

/// <summary>
/// EF InMemory context for service tests. InMemory has no transactions and no
/// <c>ExecuteUpdate</c>/<c>ExecuteDelete</c>; code paths that use those need a real database.
/// </summary>
internal static class TestDb
{
    public static AppDbContext Create( string? databaseName = null )
    {
        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase( databaseName ?? Guid.NewGuid().ToString() )
            .ConfigureWarnings( w => w.Ignore( InMemoryEventId.TransactionIgnoredWarning ) )
            .Options;
        return new AppDbContext( options );
    }
}
