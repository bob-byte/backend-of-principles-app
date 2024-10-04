using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SET.Shared.Models;

namespace SET.DataAccess.EntityConfigurations;

internal class UserHabitReminderConfigurations : IEntityTypeConfiguration<UserHabitReminder>
{
    public void Configure( EntityTypeBuilder<UserHabitReminder> builder )
    {
        builder.ToTable( name: nameof(AppDbContext.UserHabitReminders), Schemas.HABITS );

        builder.HasKey( a => a.Id );

        builder.Property( u => u.Id ).
            HasColumnType( "bigint" ).
            HasDefaultValueSql( $"nextval('{Schemas.HABITS}.sq__user_habit_reminders')" ).
            IsRequired();

        builder.Property( c => c.Title ).
            IsRequired();

        builder.Property( c => c.Description ).
            IsRequired();

        builder.Property( c => c.Time ).
            IsRequired();

        builder.HasMany( uh => uh.DaysOfWeek )
            .WithMany( wd => wd.Reminders );
    }
}
