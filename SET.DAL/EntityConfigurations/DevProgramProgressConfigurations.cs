using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SET.DataAccess;

using SET.Shared.Models;

namespace SET.DataAccess.EntityConfigurations;

internal class DevProgramProgressConfigurations : IEntityTypeConfiguration<DevProgramProgress>
{
    public void Configure(EntityTypeBuilder<DevProgramProgress> builder)
    {
        builder.ToTable( name: "DevProgramsProgresses", schema: "dev" );

        builder.Property( c => c.Id ).
            HasColumnType( DbmsConstants.UNIQUE_IDENTIFIER ).
            ValueGeneratedOnAdd();

        builder.Property( c => c.ChangedInPercent )
            .HasColumnType( typeName: "decimal(11, 8)" );

        builder.Property( c => c.TotalHabitsProgress )
            .HasColumnType( "decimal(18, 8)" );

        builder.Property( c => c.TotalProgressOfGoals )
            .HasColumnType( "decimal(18, 8)" );
    }
}
