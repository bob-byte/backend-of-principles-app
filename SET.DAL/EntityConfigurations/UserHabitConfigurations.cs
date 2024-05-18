using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SET.DataAccess;

using SET.Shared.Models;

namespace SET.DataAccess.EntityConfigurations;

internal class UserHabitConfigurations : IEntityTypeConfiguration<UserHabit>
{
    public void Configure(EntityTypeBuilder<UserHabit> builder)
    {
        builder.ToTable( name: "UserHabits", Schemas.HABITS );

        builder.HasKey( u => u.Id );

        builder.Property( u => u.Id ).
            HasColumnType( "bigint" ).
            HasDefaultValueSql( $"nextval('{Schemas.HABITS}.sq__user_habits')" );

        builder.Property( u => u.Name ).
            HasMaxLength( maxLength: 255 ).
            IsRequired();

        builder.Property( u => u.PercentageAchieved )
            .HasColumnType( typeName: "decimal(17, 16)" );

        builder.Property( u => u.ReasonToFollow ).
            IsRequired();

        builder.Property( u => u.Complexity ).
            IsRequired();

        builder.Property( u => u.Priority ).
            HasDefaultValue( 0 );

        builder.Property( u => u.Status ).
            HasDefaultValue( StatusOfHabit.InProgress );

        builder.Property( u => u.ColorName ).
            HasMaxLength(10).
            IsRequired();

        builder.HasMany<UserHabit>( u => u.ParentHabits ).
            WithMany( u => u.SubHabits );

        builder.HasMany<ProgressOfHabit>( u => u.Progresses ).
            WithOne( p => p.Habit ).
            IsRequired().
            OnDelete(DeleteBehavior.Cascade);
    }
}
