using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SET.Shared.Models;

namespace SET.DataAccess.EntityConfigurations;

public class AiConversationConfiguration : IEntityTypeConfiguration<AiConversation>
{
    public void Configure( EntityTypeBuilder<AiConversation> builder )
    {
        builder.ToTable( name: "AiConversations", Schemas.APP );
        builder.HasKey( x => x.Id );
        builder.Property( x => x.Id )
            .ValueGeneratedOnAdd();
        builder.Property( x => x.ClientId )
            .IsRequired()
            .HasMaxLength( 64 );
        builder.Property( x => x.Title )
            .IsRequired()
            .HasMaxLength( 255 );
        builder.Property( x => x.UserId )
            .IsRequired();
        builder.Property( x => x.CreatedAt )
            .IsRequired();
        builder.Property( x => x.UpdatedAt )
            .IsRequired();
        builder.HasIndex( x => x.UserId );
        builder.HasIndex( x => new { x.UserId, x.ClientId } ).IsUnique();
        builder.HasOne( x => x.User )
            .WithMany()
            .HasForeignKey( x => x.UserId )
            .OnDelete( DeleteBehavior.Cascade );
        builder.HasMany( x => x.Messages )
            .WithOne( x => x.Conversation )
            .HasForeignKey( x => x.ConversationId )
            .OnDelete( DeleteBehavior.Cascade );
    }
}
