using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SET.DataAccess;
using SET.Shared.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET.DataAccess.EntityConfigurations;

internal class FrequencyConfigurations : IEntityTypeConfiguration<Frequency>
{
    public void Configure( EntityTypeBuilder<Frequency> builder )
    {
        builder.ToTable( name: nameof(AppDbContext.Frequencies), Schemas.NOTICES );

        builder.Property( f => f.Value ).HasColumnType( "decimal(7, 6)" );
        builder.HasMany( f => f.Habits ).
            WithOne( u => u.Frequency ).
            IsRequired().
            OnDelete(DeleteBehavior.Restrict);
    }
}
