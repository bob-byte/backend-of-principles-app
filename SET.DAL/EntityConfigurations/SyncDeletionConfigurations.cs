using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SET.Shared.Models;

namespace SET.DataAccess.EntityConfigurations;

public class SyncDeletionConfigurations : IEntityTypeConfiguration<SyncDeletion>
{
    public void Configure( EntityTypeBuilder<SyncDeletion> builder )
    {
        builder.ToTable( name: "SyncDeletions", Schemas.APP );
        builder.HasKey( x => x.Id );
        builder.Property( x => x.Id )
            .HasDefaultValueSql( $"nextval('{Schemas.APP}.sq__sync_deletions')" );
        builder.Property( x => x.EntityType )
            .IsRequired()
            .HasMaxLength( 32 );
        builder.Property( x => x.EntityId )
            .IsRequired();
        builder.Property( x => x.DeletedAt )
            .IsRequired();
        builder.HasIndex( x => new { x.UserId, x.DeletedAt } );
        builder.HasIndex( x => new { x.UserId, x.EntityType, x.EntityId } );
        builder.HasOne( x => x.User )
            .WithMany()
            .HasForeignKey( x => x.UserId )
            .OnDelete( DeleteBehavior.Cascade );
    }
}
