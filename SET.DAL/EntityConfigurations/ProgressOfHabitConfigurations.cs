using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SET.Shared.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET.DataAccess.EntityConfigurations;

internal class ProgressOfHabitConfigurations : IEntityTypeConfiguration<ProgressOfHabit>
{
    public void Configure( EntityTypeBuilder<ProgressOfHabit> builder )
    {
        builder.ToTable( name: nameof( AppDbContext.ProgressesOfHabits ), Schemas.HABITS );

        builder.HasKey( u => u.Id );

        builder.Property( u => u.Id ).
            HasColumnType( "bigint" ).
            HasDefaultValueSql( "nextval('sq_progressesofhabits')" );
    }
}
