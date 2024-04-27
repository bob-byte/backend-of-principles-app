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

        builder.Property(u => u.Id).
            ValueGeneratedOnAdd().
            HasColumnType(DbmsConstants.UNIQUE_IDENTIFIER);

        builder.Property(u => u.Name).
            IsRequired();

        builder.Property(u => u.ColorName).
            IsRequired();
    }
}
