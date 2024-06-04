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
    public DbSet<UserAreaOfLife> UserAreasOfLife { get; set; }
    public DbSet<UserAreaOfLifeUserHabit> UserAreasOfLifeUserHabits { get; set; }
    public DbSet<UserHabit> UserHabits { get; set; }
    public DbSet<ProgressOfHabit> ProgressesOfHabits { get; set; }
    public DbSet<Frequency> Frequencies { get; set; }
    public DbSet<ClientLog> ClientLogs { get; set; }
    public DbSet<UserPrinciple> UserPrinciples { get; set; }
    public DbSet<PrincipleProgress> PrincipleProgresses { get; set; }

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
        modelBuilder.ApplyConfiguration( new ClientLogConfigurations() );
        modelBuilder.ApplyConfiguration( new UserPrincipleConfigurations() );
        modelBuilder.ApplyConfiguration( new PrincipleProgressConfigurations() );

        modelBuilder.HasSequence<long>( "sq__user_areas_of_life_user_habits", Schemas.AREA_OF_LIFE ).
        StartsAt( 100 ).
        IncrementsBy( 1 );

        modelBuilder.HasSequence<long>( "sq__user_areas_of_life", Schemas.AREA_OF_LIFE ).
        StartsAt( 100 ).
        IncrementsBy( 1 );

        modelBuilder.HasSequence<long>( "sq__file_entities", Schemas.APP ).
        StartsAt( 100 ).
        IncrementsBy( 1 );

        modelBuilder.HasSequence<long>( "sq__users", Schemas.APP ).
        StartsAt( 100 ).
        IncrementsBy( 1 );

        modelBuilder.HasSequence<long>( "sq__user_habits", Schemas.HABITS ).
        StartsAt( 100 ).
        IncrementsBy( 1 );

        modelBuilder.HasSequence<long>( "sq__frequencies", Schemas.APP ).
        StartsAt( 100 ).
        IncrementsBy( 1 );

        modelBuilder.HasSequence<long>( "sq__progresses_of_habits", Schemas.HABITS ).
        StartsAt( 100 ).
        IncrementsBy( 1 );

        modelBuilder.HasSequence<long>( "sq__statements", Schemas.APP ).
        StartsAt( 100 ).
        IncrementsBy( 1 );

        modelBuilder.HasSequence<long>( "sq__client_logs", Schemas.APP ).
        StartsAt( 100 ).
        IncrementsBy( 1 );

        modelBuilder.HasSequence<long>( "sq__user_principles", Schemas.PRINCIPLES ).
        StartsAt( 100 ).
        IncrementsBy( 1 );

        modelBuilder.HasSequence<long>( "sq__principle_progresses", Schemas.PRINCIPLES ).
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
        connectionString = "Host=principles_database;Port=5432;Username=postgres;Password=76193db1d01e34743d5c;Database=principles;";
#endif
        optionsBuilder.UseNpgsql( connectionString );
    }
}
