using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SET.Shared.Models;

namespace SET.DataAccess.EntityConfigurations;

internal class UserPrincipleConfigurations : IEntityTypeConfiguration<UserPrinciple>
{
    public void Configure( EntityTypeBuilder<UserPrinciple> builder )
    {
        builder.ToTable( name: "UserPrinciples", Schemas.PRINCIPLES );

        builder.HasKey( a => a.Id );

        builder.Property( u => u.Id ).
            HasColumnType( "bigint" ).
            HasDefaultValueSql( $"nextval('{Schemas.PRINCIPLES}.sq__user_principles')" ).
            IsRequired();

        builder.Property( u => u.ReasonToFollow ).
            IsRequired();
    }
}
