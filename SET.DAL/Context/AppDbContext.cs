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
    public DbSet<UserAreaOfLife> UserAreasOfLife { get; set; }
    public DbSet<UserAreaOfLifeUserHabit> UserAreasOfLifeUserHabits { get; set; }
    public DbSet<UserHabit> UserHabits { get; set; }
    public DbSet<ProgressOfHabit> ProgressesOfHabits { get; set; }
    public DbSet<Frequency> Frequencies { get; set; }
    public DbSet<ClientLog> ClientLogs { get; set; }
    public DbSet<UserGoal> UserGoals { get; set; }
    public DbSet<UserReminder> UserReminders { get; set; }
    public DbSet<WeekDay> WeekDays { get; set; }
    public DbSet<UserHabitReminder> UserHabitReminders { get; set; }
    public DbSet<TrackingOfUserNotificationRequests> TrackingOfUserNotificationRequests { get; set; }
    public DbSet<Task> Tasks { get; set; }
    public DbSet<TaskSubtask> TaskSubtasks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration( new UserConfigurations() );
        modelBuilder.ApplyConfiguration( new FrequencyConfigurations() );
        modelBuilder.ApplyConfiguration( new UserAreaOfLifeUserHabitConfigurations() );
        modelBuilder.ApplyConfiguration( new UserAreaOfLifeConfigurations() );
        modelBuilder.ApplyConfiguration( new UserHabitConfigurations() );
        modelBuilder.ApplyConfiguration( new ProgressOfHabitConfigurations() );
        modelBuilder.ApplyConfiguration( new ClientLogConfigurations() );
        modelBuilder.ApplyConfiguration( new UserGoalConfigurations() );
        modelBuilder.ApplyConfiguration( new UserReminderConfigurations() );
        modelBuilder.ApplyConfiguration( new WeekDayConfigurations() );
        modelBuilder.ApplyConfiguration( new UserHabitReminderConfigurations() );
        modelBuilder.ApplyConfiguration( new TrackingOfUserNotificationRequestsConfigurations() );
        modelBuilder.ApplyConfiguration( new TasksConfiguration());
        modelBuilder.ApplyConfiguration( new TaskSubtasksConfiguration());

        modelBuilder.HasSequence<long>( "sq__user_areas_of_life_user_habits", Schemas.AREA_OF_LIFE ).
        StartsAt( 100 ).
        IncrementsBy( 1 );

        modelBuilder.HasSequence<long>( "sq__user_areas_of_life", Schemas.AREA_OF_LIFE ).
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

        modelBuilder.HasSequence<long>( "sq__client_logs", Schemas.APP ).
        StartsAt( 100 ).
        IncrementsBy( 1 );

        modelBuilder.HasSequence<long>( "sq__user_goals", Schemas.GOAL ).
        StartsAt( 100 ).
        IncrementsBy( 1 );

        modelBuilder.HasSequence<long>( "sq__user_reminders", Schemas.APP ).
        StartsAt( 100 ).
        IncrementsBy( 1 );

        modelBuilder.HasSequence<long>( "sq__week_days", Schemas.APP ).
        StartsAt( 100 ).
        IncrementsBy( 1 );

        modelBuilder.HasSequence<long>( "sq__user_habit_reminders", Schemas.HABITS ).
        StartsAt( 100 ).
        IncrementsBy( 1 );

        modelBuilder.HasSequence<long>( "sq__tracking__of__user__notification__requests", Schemas.APP ).
        StartsAt( 100 ).
        IncrementsBy( 1 );
    }

    protected override void OnConfiguring( DbContextOptionsBuilder optionsBuilder )
    {
        if (optionsBuilder.IsConfigured)
        {
            return;
        }

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
