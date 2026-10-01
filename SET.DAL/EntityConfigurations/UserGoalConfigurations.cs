using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SET.Shared.Models;

namespace SET.DataAccess.EntityConfigurations;

internal class UserGoalConfigurations : IEntityTypeConfiguration<UserGoal>
{
    public void Configure( EntityTypeBuilder<UserGoal> builder )
    {
        builder.ToTable( name: nameof(AppDbContext.UserGoals), Schemas.GOAL );

        builder.HasKey( a => a.Id );

        builder.Property( u => u.Id ).
            HasColumnType( "bigint" ).
            HasDefaultValueSql( $"nextval('{Schemas.GOAL}.sq__user_goals')" ).
            IsRequired();

        builder.Property( c => c.Name ).
            IsRequired();

        builder.Property( c => c.Notes ).
            IsRequired( false ).
            HasColumnType( "text" );

        builder.Property( u => u.UserId )
            .HasColumnType( "bigint" )
            .IsRequired();

        builder.Property( u => u.CreatedAt ).
            HasDefaultValueSql( "now()" ).
            IsRequired();

        builder.Property( u => u.UpdatedAt ).
            IsRequired(false);

        builder.Property( u => u.IsCompleted ).
            IsRequired().
            HasDefaultValue( false );

        builder.HasMany( u => u.UserHabits ).
            WithOne( u => u.Goal ).
            HasForeignKey( u => u.GoalId ).
            IsRequired( false ).
            OnDelete( DeleteBehavior.Restrict );
    }
}
