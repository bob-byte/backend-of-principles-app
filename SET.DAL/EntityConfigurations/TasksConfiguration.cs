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
           .HasMaxLength(255);
        builder.Property(x => x.Date)
           .IsRequired(false);
        builder.Property(x => x.Time)
           .IsRequired(false);
        builder.HasOne(x => x.User)
           .WithMany()
           .HasForeignKey(x => x.UserId)
           .OnDelete(DeleteBehavior.Cascade);
    }
}