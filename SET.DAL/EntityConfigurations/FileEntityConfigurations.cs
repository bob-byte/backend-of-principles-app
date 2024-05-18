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
internal class FileEntityConfigurations : IEntityTypeConfiguration<FileEntity>
{
    public void Configure( EntityTypeBuilder<FileEntity> builder )
    {
        builder.ToTable( name: "FileEntities", Schemas.APP );

        builder.HasKey( a => a.Id );

        builder.Property( u => u.Id ).
            HasColumnType( "bigint" ).
            HasDefaultValueSql( $"nextval('{Schemas.APP}.sq__file_entities')" );
    }
}
