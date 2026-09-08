using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SET.Shared.Models;

namespace SET.DataAccess.EntityConfigurations;

public class AiMessageConfiguration : IEntityTypeConfiguration<AiMessage>
{
    public void Configure( EntityTypeBuilder<AiMessage> builder )
    {
        builder.ToTable( name: "AiMessages", Schemas.APP );
        builder.HasKey( x => x.Id );
        builder.Property( x => x.Id )
            .ValueGeneratedOnAdd();
        builder.Property( x => x.ClientId )
            .IsRequired()
            .HasMaxLength( 64 );
        builder.Property( x => x.Role )
            .IsRequired()
            .HasMaxLength( 32 );
        builder.Property( x => x.Content )
            .IsRequired()
            .HasColumnType( "text" );
        builder.Property( x => x.SortOrder )
            .IsRequired()
            .HasDefaultValue( 0 );
        builder.Property( x => x.CreatedAt )
            .IsRequired();
        builder.HasIndex( x => x.ConversationId );
        builder.HasIndex( x => new { x.ConversationId, x.ClientId } ).IsUnique();
        builder.HasOne( x => x.Conversation )
            .WithMany( c => c.Messages )
            .HasForeignKey( x => x.ConversationId )
            .OnDelete( DeleteBehavior.Cascade );
    }
}
