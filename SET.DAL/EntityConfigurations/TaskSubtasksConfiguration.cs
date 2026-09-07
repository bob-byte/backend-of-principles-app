using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SET.Shared.Models;

namespace SET.DataAccess.EntityConfigurations;

public class TaskSubtasksConfiguration : IEntityTypeConfiguration<TaskSubtask>
{
    public void Configure(EntityTypeBuilder<TaskSubtask> builder)
    {
        builder.ToTable(name: "TaskSubtasks", Schemas.TSK);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();
        builder.Property(x => x.ClientId)
            .IsRequired()
            .HasMaxLength(64);
        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(255);
        builder.Property(x => x.IsCompleted)
            .IsRequired()
            .HasDefaultValue(false);
        builder.Property(x => x.SortOrder)
            .IsRequired()
            .HasDefaultValue(0);
        builder.HasIndex(x => x.TaskId);
        builder.HasIndex(x => new { x.TaskId, x.ClientId }).IsUnique();
        builder.HasOne(x => x.Task)
            .WithMany(t => t.Subtasks)
            .HasForeignKey(x => x.TaskId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
