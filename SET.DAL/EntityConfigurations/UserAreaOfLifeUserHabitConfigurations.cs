using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SET.Shared.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET.DataAccess.EntityConfigurations;

public class UserAreaOfLifeUserHabitConfigurations : IEntityTypeConfiguration<UserAreaOfLifeUserHabit>
{
    public void Configure( EntityTypeBuilder<UserAreaOfLifeUserHabit> builder )
    {
        builder.ToTable( name: "UserAreasOfLifeUserHabits", Schemas.AREA_OF_LIFE );

        builder.Property( u => u.Id ).
            ValueGeneratedOnAdd().
            HasColumnType(DbmsConstants.UNIQUE_IDENTIFIER);


        builder.HasIndex( u => u.AreaOfLifeId );

        builder.HasOne<UserAreaOfLife>( u => u.AreaOfLife )
            .WithMany( u => u.Habits )
            .HasForeignKey( u => u.AreaOfLifeId );


        builder.HasIndex( u => u.HabitId );

        builder.HasOne<UserHabit>( u => u.Habit ).
            WithMany( u => u.AreasOfLife ).
            HasForeignKey( u => u.HabitId );
    }
}
