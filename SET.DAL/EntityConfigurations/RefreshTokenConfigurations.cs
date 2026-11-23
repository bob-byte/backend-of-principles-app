using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SET.Shared.Models;

namespace SET.DataAccess.EntityConfigurations;

internal class RefreshTokenConfigurations : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure( EntityTypeBuilder<RefreshToken> builder )
    {
        builder.ToTable( name: "RefreshTokens", Schemas.APP );

        builder.HasKey( x => x.Id );

        builder.Property( x => x.Id )
            .HasColumnType( "bigint" )
            .HasDefaultValueSql( $"nextval('{Schemas.APP}.sq__refresh_tokens')" );

        builder.Property( x => x.TokenHash )
            .IsRequired()
            .HasMaxLength( 64 );

        builder.Property( x => x.ExpiresAt )
            .HasColumnType( "timestamp with time zone" )
            .IsRequired();

        builder.Property( x => x.CreatedAt )
            .HasColumnType( "timestamp with time zone" )
            .IsRequired();

        builder.Property( x => x.RevokedAt )
            .HasColumnType( "timestamp with time zone" );

        builder.Property( x => x.ReplacedByTokenHash )
            .HasMaxLength( 64 );

        builder.Property( x => x.DeviceId )
            .HasMaxLength( 64 );

        builder.HasIndex( x => x.TokenHash )
            .IsUnique();

        builder.HasIndex( x => x.UserId );

        builder.HasOne( x => x.User )
            .WithMany()
            .HasForeignKey( x => x.UserId )
            .OnDelete( DeleteBehavior.Cascade );
    }
}
