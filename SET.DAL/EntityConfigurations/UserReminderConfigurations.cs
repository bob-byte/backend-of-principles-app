using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SET.Shared.Models;

namespace SET.DataAccess.EntityConfigurations;

internal class UserReminderConfigurations : IEntityTypeConfiguration<UserReminder>
{
    public void Configure( EntityTypeBuilder<UserReminder> builder )
    {
        builder.ToTable( name: nameof(AppDbContext.UserReminders), Schemas.APP );

        builder.HasKey( a => a.Id );

        builder.Property( u => u.Id ).
            HasColumnType( "bigint" ).
            HasDefaultValueSql( $"nextval('{Schemas.APP}.sq__user_reminders')" ).
            IsRequired();

        builder.Property( c => c.Title ).
            IsRequired();

        builder.Property( c => c.Description ).
            IsRequired();

        builder.Property( c => c.Time ).
            IsRequired();

        builder.Property( c => c.UserNotificationRequestId );
    }
}
