using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SET.Shared.Models;

namespace SET.DataAccess.EntityConfigurations;

internal class WeekDayConfigurations : IEntityTypeConfiguration<WeekDay>
{
    public void Configure( EntityTypeBuilder<WeekDay> builder )
    {
        builder.ToTable( name: nameof(AppDbContext.WeekDays), Schemas.APP );

        builder.HasKey( a => a.Id );

        builder.Property( u => u.Id ).
            HasColumnType( "bigint" ).
            HasDefaultValueSql( $"nextval('{Schemas.APP}.sq__week_days')" ).
            IsRequired();

        builder.Property( c => c.Type ).
            IsRequired();
    }
}
