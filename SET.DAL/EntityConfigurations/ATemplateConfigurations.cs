using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SET.DataAccess;
using SET.Shared.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET.DataAccess.EntityConfigurations;

internal class ATemplateConfigurations : IEntityTypeConfiguration<User>
{
    public void Configure( EntityTypeBuilder<User> builder )
    {
        builder.ToTable( name: nameof(AppDbContext.Users), Schemas.APP );

        builder.Property( u => u.Id ).
            IsRequired();
    }
}
