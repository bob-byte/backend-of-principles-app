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

    public DbSet<Book> Books { get; set; }
    public DbSet<BuiltInFrequency> BuiltInFrequencies { get; set; }

    public DbSet<Challenge> Challenges { get; set; }
    public DbSet<Diet> Diets { get; set; }
    public DbSet<EndRepeat> EndRepeats { get; set; }
    public DbSet<Goal> Goals { get; set; }
    public DbSet<Notice> Notices { get; set; }
    public DbSet<Rd71> Rd71s { get; set; }
    public DbSet<Reading> Readings { get; set; }
    public DbSet<Recipe> Recipes { get; set; }
    public DbSet<Reminder> Reminders { get; set; }
    public DbSet<NoticeRepeat> NoticeRepeats { get; set; }
    public DbSet<TimeZone> TimeZones { get; set; }
    public DbSet<TrainingProgram> TrainingPrograms { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserFrequency> UserFrequencies { get; set; }
    public DbSet<UserTask> UserTasks { get; set; }
    public DbSet<Workout> Workouts { get; set; }
    public DbSet<DevelopmentPlan> DevelopmentPlans { get; set; }
    public DbSet<FileEntity> FileEntities { get; set; }
    public DbSet<Statement> Statements { get; set; }
    public DbSet<RecomendedBook> RecomendedBooks { get; set; }
    public DbSet<ComplicatedDevProgram> ComplicatedDevPrograms { get; set; }
    //public DbSet<ConfiguredDevProgram> ConfiguredDevPrograms { get; set; }
    public DbSet<DevProgramProgress> DevProgramProgresses { get; set; }
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

        modelBuilder.ApplyConfiguration( configuration: new ChallengeConfigurations() );
        modelBuilder.ApplyConfiguration( new UserConfigurations() );
        modelBuilder.ApplyConfiguration( new FrequencyConfigurations() );
        modelBuilder.ApplyConfiguration( new UserAreaOfLifeUserHabitConfigurations() );
        modelBuilder.ApplyConfiguration( new UserAreaOfLifeConfigurations() );
        modelBuilder.ApplyConfiguration( new UserHabitConfigurations() );
        modelBuilder.ApplyConfiguration( new ProgressOfHabitConfigurations() );
        modelBuilder.ApplyConfiguration( new ComplicatedDevProgramConfigurations() );
        modelBuilder.ApplyConfiguration( new DevProgramProgressConfigurations() );
    }

    protected override void OnConfiguring( DbContextOptionsBuilder optionsBuilder )
    {
        base.OnConfiguring( optionsBuilder );
        string connectionString;
#if DEBUG
        connectionString = "Server=localhost;Database=SET;User=sa;Password=76FE5bs6rG;TrustServerCertificate=True;Connect Timeout=30;Encrypt=True";
#else
        connectionString = "Server=habitsmentorsetdbserver.database.windows.net;Initial Catalog=SET;Persist Security Info=False;User ID=habitsmentorset;Password=#1927Bodya;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
#endif
        optionsBuilder.UseSqlServer(connectionString, builder => builder.UseDateOnlyTimeOnly());
    }
}
