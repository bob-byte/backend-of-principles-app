using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SET.DataAccess;

using SET.Shared.Models;

namespace SET.DataAccess.EntityConfigurations;

internal class ComplicatedDevProgramConfigurations : IEntityTypeConfiguration<ComplicatedDevProgram>
{
    public void Configure(EntityTypeBuilder<ComplicatedDevProgram> builder)
    {
        builder.ToTable( name: "ComplicatedDevPrograms", schema: "dev" );

        builder.Property( c => c.Id ).
            HasColumnType( DbmsConstants.UNIQUE_IDENTIFIER ).
            ValueGeneratedOnAdd();

        builder.Property( c => c.ShortDescription ).
            IsRequired();

        builder.Property(c => c.Name).
            HasMaxLength(maxLength: 255).
            IsRequired();
    }
}
