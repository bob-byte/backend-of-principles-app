using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SET.DataAccess;
using SET.Shared.Models;

namespace SET.DataAccess.EntityConfigurations;

internal class UserAreaOfLifeConfigurations : IEntityTypeConfiguration<UserAreaOfLife>
{
    public void Configure(EntityTypeBuilder<UserAreaOfLife> builder)
    {
        builder.ToTable( name: "UserAreasOfLife", Schemas.AREA_OF_LIFE);

        builder.HasKey(a => a.Id);

        builder.Property( u => u.Id ).
            HasColumnType( "bigint" ).
            HasDefaultValueSql( $"nextval('{Schemas.AREA_OF_LIFE}.sq__user_areas_of_life')" );

        builder.Property(u => u.Name).
            IsRequired();
    }
}
