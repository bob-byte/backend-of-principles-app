using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SET.DataAccess;

using SET.Shared.Models;

namespace SET.DataAccess.EntityConfigurations;

internal class UserConfigurations : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable( name: nameof( AppDbContext.Users ), Schemas.USERS );

        builder.HasKey( u => u.Id );

        builder.Property( u => u.Id ).
            HasColumnType( "bigint" ).
            HasDefaultValueSql( "nextval('sq_users')" );

        builder.Property(c => c.Name).
            IsRequired();

        builder.Property(c => c.Email).
            IsRequired();

        builder.Property(c => c.Password).
            HasColumnType(DbmsConstants.BYTE_ARRAY_TYPE).
            HasMaxLength(255).
            IsRequired();

        builder.Property( c => c.MainSlogan ).
            HasMaxLength( 255 );

        builder.HasMany( u => u.AreasOfLife ).
            WithOne( u => u.User ).
            IsRequired().
            OnDelete(DeleteBehavior.Cascade);

        builder.HasMany( u => u.Habits ).
            WithOne( u => u.User ).
            HasForeignKey(u => u.UserId).
            IsRequired().
            OnDelete( DeleteBehavior.NoAction );
    }
}
