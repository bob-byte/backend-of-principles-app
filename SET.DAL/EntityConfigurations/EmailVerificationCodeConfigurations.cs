using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SET.Shared.Models;

namespace SET.DataAccess.EntityConfigurations;

internal class EmailVerificationCodeConfigurations : IEntityTypeConfiguration<EmailVerificationCode>
{
    public void Configure( EntityTypeBuilder<EmailVerificationCode> builder )
    {
        builder.ToTable( name: "EmailVerificationCodes", Schemas.APP );

        builder.HasKey( x => x.Id );

        builder.Property( x => x.Id )
            .HasColumnType( "bigint" )
            .HasDefaultValueSql( $"nextval('{Schemas.APP}.sq__email_verification_codes')" );

        builder.Property( x => x.Email )
            .IsRequired()
            .HasMaxLength( 256 );

        builder.Property( x => x.Purpose )
            .IsRequired()
            .HasMaxLength( 32 );

        builder.Property( x => x.CodeHash )
            .IsRequired()
            .HasMaxLength( 64 );

        builder.Property( x => x.ExpiresAt )
            .HasColumnType( "timestamp with time zone" )
            .IsRequired();

        builder.Property( x => x.CreatedAt )
            .HasColumnType( "timestamp with time zone" )
            .IsRequired();

        builder.HasIndex( x => new { x.Email, x.Purpose } )
            .IsUnique();
    }
}
