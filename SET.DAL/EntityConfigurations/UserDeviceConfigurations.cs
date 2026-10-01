using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SET.Shared.Models;

namespace SET.DataAccess.EntityConfigurations;

public class UserDeviceConfigurations : IEntityTypeConfiguration<UserDevice>
{
    public void Configure( EntityTypeBuilder<UserDevice> builder )
    {
        builder.ToTable( name: "UserDevices", Schemas.APP );
        builder.HasKey( x => x.Id );
        builder.Property( x => x.Id )
            .HasDefaultValueSql( $"nextval('{Schemas.APP}.sq__user_devices')" );
        builder.Property( x => x.DeviceId )
            .IsRequired()
            .HasMaxLength( 64 );
        builder.Property( x => x.PushToken )
            .IsRequired()
            .HasMaxLength( 512 );
        builder.Property( x => x.Platform )
            .IsRequired()
            .HasMaxLength( 16 );
        builder.Property( x => x.CreatedAt )
            .IsRequired();
        builder.Property( x => x.UpdatedAt )
            .IsRequired();
        builder.HasIndex( x => x.DeviceId )
            .IsUnique();
        builder.HasIndex( x => x.UserId );
        builder.HasOne( x => x.User )
            .WithMany()
            .HasForeignKey( x => x.UserId )
            .OnDelete( DeleteBehavior.Cascade );
    }
}
