using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SET.Shared.Models;

namespace SET.DataAccess.EntityConfigurations;

public class TasksConfiguration: IEntityTypeConfiguration<Task>{
    public void Configure(EntityTypeBuilder<Task> builder){
        builder.ToTable(name: "Tasks", Schemas.TSK);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name)
           .IsRequired()
           .HasMaxLength(255);
        builder.Property(x => x.UserId)
           .IsRequired();
        builder.Property(x => x.Notes)
           .IsRequired(false)
           .HasColumnType("text");
        builder.Property(x => x.Date)
           .IsRequired(false);
        builder.Property(x => x.Time)
           .IsRequired(false);
        builder.Property(x => x.EndDate)
           .IsRequired(false);
        builder.Property(x => x.EndTime)
           .IsRequired(false);
        builder.Property(x => x.AllDay)
           .IsRequired()
           .HasDefaultValue(false);
        builder.Property(x => x.IsCompleted)
               .IsRequired()
               .HasDefaultValue(false);
        builder.Property(x => x.ConstantReminder)
               .IsRequired()
               .HasDefaultValue(false);
        builder.Property(x => x.ConstantNotificationRequestId)
               .IsRequired(false);
        builder.Property(x => x.RemindersJson)
               .IsRequired(false)
               .HasColumnType("text");
        builder.Property(x => x.RepeatJson)
               .IsRequired(false)
               .HasColumnType("text");
        builder.HasOne(x => x.User)
           .WithMany()
           .HasForeignKey(x => x.UserId)
           .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Subtasks)
           .WithOne(x => x.Task)
           .HasForeignKey(x => x.TaskId)
           .OnDelete(DeleteBehavior.Cascade);
    }
}
