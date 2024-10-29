using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SET.Shared.Models;

namespace SET.DataAccess.EntityConfigurations;

internal class TrackingOfUserNotificationRequestsConfigurations : IEntityTypeConfiguration<TrackingOfUserNotificationRequests>
{
    public void Configure( EntityTypeBuilder<TrackingOfUserNotificationRequests> builder )
    {
        builder.ToTable( name: nameof(AppDbContext.TrackingOfUserNotificationRequests), Schemas.APP );

        builder.HasKey( a => a.Id );

        builder.Property( u => u.Id ).
            HasColumnType( "bigint" ).
            HasDefaultValueSql( $"nextval('{Schemas.APP}.sq__tracking__of__user__notification__requests')" ).
            IsRequired();

        builder.Property(u => u.MaxNotificationRequestId).
            IsRequired();
    }
}
