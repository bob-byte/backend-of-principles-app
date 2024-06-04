using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SET.Shared.Models;

namespace SET.DataAccess.EntityConfigurations;

internal class PrincipleProgressConfigurations : IEntityTypeConfiguration<PrincipleProgress>
{
    public void Configure( EntityTypeBuilder<PrincipleProgress> builder )
    {
        builder.ToTable( name: "PrincipleProgresses", Schemas.PRINCIPLES );

        builder.HasKey( a => a.Id );

        builder.Property( u => u.Id ).
            HasColumnType( "bigint" ).
            HasDefaultValueSql( $"nextval('{Schemas.PRINCIPLES}.sq__principle_progresses')" ).
            IsRequired();

        builder.Property( u => u.PrincipleId ).
            IsRequired();

        builder.HasOne( u => u.Principle ).
        WithMany( p => p.Progresses ).
        IsRequired().
        HasForeignKey( u => u.PrincipleId );
    }
}
