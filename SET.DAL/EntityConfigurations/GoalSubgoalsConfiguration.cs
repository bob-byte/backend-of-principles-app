using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SET.Shared.Models;

namespace SET.DataAccess.EntityConfigurations;

public class GoalSubgoalsConfiguration : IEntityTypeConfiguration<GoalSubgoal>
{
    public void Configure( EntityTypeBuilder<GoalSubgoal> builder )
    {
        builder.ToTable( name: "GoalSubgoals", Schemas.GOAL );
        builder.HasKey( x => x.Id );
        builder.Property( x => x.Id )
            .ValueGeneratedOnAdd();
        builder.Property( x => x.ClientId )
            .IsRequired()
            .HasMaxLength( 64 );
        builder.Property( x => x.Name )
            .IsRequired()
            .HasMaxLength( 255 );
        builder.Property( x => x.IsCompleted )
            .IsRequired()
            .HasDefaultValue( false );
        builder.Property( x => x.SortOrder )
            .IsRequired()
            .HasDefaultValue( 0 );
        builder.HasIndex( x => x.GoalId );
        builder.HasIndex( x => new { x.GoalId, x.ClientId } ).IsUnique();
        builder.HasOne( x => x.Goal )
            .WithMany( g => g.Subgoals )
            .HasForeignKey( x => x.GoalId )
            .OnDelete( DeleteBehavior.Cascade );
    }
}
