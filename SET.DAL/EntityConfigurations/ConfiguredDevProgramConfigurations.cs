using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SET.Shared.Models;

namespace SET.DataAccess.EntityConfigurations;

internal class ConfiguredDevProgramConfigurations : IEntityTypeConfiguration<ConfiguredDevProgram>
{
    public void Configure(EntityTypeBuilder<ConfiguredDevProgram> builder)
    {
        builder.ToTable( name: "ConfiguredDevPrograms", schema: "dev" );

        builder.Property( c => c.Id ).
            HasColumnType( DbmsConstants.UNIQUE_IDENTIFIER ).
            ValueGeneratedOnAdd();

        builder.Property(c => c.Name).
            HasMaxLength(maxLength: 255).
            IsRequired();
    }
}
