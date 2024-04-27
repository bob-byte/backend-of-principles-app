using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SET.DataAccess;

using SET.Shared.Models;

namespace SET.DataAccess.EntityConfigurations;

internal class ChallengeConfigurations : IEntityTypeConfiguration<Challenge>
{
    public void Configure(EntityTypeBuilder<Challenge> builder)
    {
        builder.Property( c => c.Id ).
            HasColumnType( DbmsConstants.UNIQUE_IDENTIFIER ).
            ValueGeneratedOnAdd();

        builder.Property( c=> c.UserId ).
            HasColumnType( DbmsConstants.UNIQUE_IDENTIFIER );

        builder.HasIndex( c => c.UserId );

        builder.Property( c => c.ShortDescription ).
            IsRequired();

        builder.Property( c => c.UrlWithFullDescription ).
            IsRequired();

        builder.Property( c => c.Foundator ).
            IsRequired();

        builder.Property(c => c.Name).
            HasMaxLength(255).
            IsRequired();

        builder.HasOne( c => c.Rd71 ).
            WithOne( c => c.Challenge ).
            HasForeignKey<Rd71>(c => c.ChallengeId);
    }
}
