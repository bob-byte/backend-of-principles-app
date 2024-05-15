using SET.DataAccess.EntityConfigurations;
using SET.DataAccess.Extensions;

using Microsoft.EntityFrameworkCore;

using SET.Shared.Models;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace SET.DataAccess;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public AppDbContext()
    {
        //do nothing
    }
    public DbSet<User> Users { get; set; }
    public DbSet<FileEntity> FileEntities { get; set; }
    public DbSet<Statement> Statements { get; set; }
    //public DbSet<ConfiguredDevProgram> ConfiguredDevPrograms { get; set; }
    public DbSet<UserAreaOfLife> UserAreasOfLife { get; set; }
    public DbSet<UserAreaOfLifeUserHabit> UserAreasOfLifeUserHabits { get; set; }
    public DbSet<UserHabit> UserHabits { get; set; }
    public DbSet<ProgressOfHabit> ProgressesOfHabits { get; set; }
    public DbSet<Frequency> Frequencies { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        bool executeSeed = false;
        if ( executeSeed )
        {
            modelBuilder.Entity<Statement>().SeedDefaultStatements();
        }
        modelBuilder.ApplyConfiguration( new UserConfigurations() );
        modelBuilder.ApplyConfiguration( new FrequencyConfigurations() );
        modelBuilder.ApplyConfiguration( new UserAreaOfLifeUserHabitConfigurations() );
        modelBuilder.ApplyConfiguration( new UserAreaOfLifeConfigurations() );
        modelBuilder.ApplyConfiguration( new UserHabitConfigurations() );
        modelBuilder.ApplyConfiguration( new ProgressOfHabitConfigurations() );
        modelBuilder.ApplyConfiguration( new FileEntityConfigurations() );
        modelBuilder.ApplyConfiguration( new StatementConfigurations() );

        modelBuilder.HasSequence<long>( "sq_userareasoflifeuserhabit" ).
        StartsAt( 100 ).
        IncrementsBy( 1 );

        modelBuilder.HasSequence<long>( "sq_userareasoflife" ).
        StartsAt( 100 ).
        IncrementsBy( 1 );

        modelBuilder.HasSequence<long>( "sq_filesentity" ).
        StartsAt( 100 ).
        IncrementsBy( 1 );

        modelBuilder.HasSequence<long>( "sq_users" ).
        StartsAt( 100 ).
        IncrementsBy( 1 );

        modelBuilder.HasSequence<long>( "sq_userhabits" ).
        StartsAt( 100 ).
        IncrementsBy( 1 );

        modelBuilder.HasSequence<long>( "sq_frequencies" ).
        StartsAt( 100 ).
        IncrementsBy( 1 );

        modelBuilder.HasSequence<long>( "sq_progressesofhabits" ).
        StartsAt( 100 ).
        IncrementsBy( 1 );

        modelBuilder.HasSequence<long>( "sq_statements" ).
        StartsAt( 100 ).
        IncrementsBy( 1 );
    }

    protected override void OnConfiguring( DbContextOptionsBuilder optionsBuilder )
    {
        base.OnConfiguring( optionsBuilder );
        string connectionString;
#if DEBUG
        connectionString = "Host=localhost;Database=SET;Port=5432;Username=postgres;Password=qwerty";
#else
        connectionString = "Server=habitsmentorsetdbserver.database.windows.net;Initial Catalog=SET;Persist Security Info=False;User ID=habitsmentorset;Password=#1927Bodya;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
#endif
        optionsBuilder.UseNpgsql( connectionString );
    }
}
