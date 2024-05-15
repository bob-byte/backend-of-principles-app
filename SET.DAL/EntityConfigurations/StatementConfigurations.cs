using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SET.Shared.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET.DataAccess.EntityConfigurations;
internal class StatementConfigurations : IEntityTypeConfiguration<Statement>
{
    public void Configure( EntityTypeBuilder<Statement> builder )
    {
        builder.ToTable( name: nameof( AppDbContext.Statements ), Schemas.STATEMENTS );

        builder.HasKey( u => u.Id );

        builder.Property( u => u.Id ).
            HasColumnType( "bigint" ).
            HasDefaultValueSql( "nextval('sq_statements')" );
    }
}
