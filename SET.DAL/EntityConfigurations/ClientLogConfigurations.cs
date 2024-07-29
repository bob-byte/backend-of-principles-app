using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SET.Shared.Models;

namespace SET.DataAccess.EntityConfigurations;

internal class ClientLogConfigurations : IEntityTypeConfiguration<ClientLog>
{
    public void Configure( EntityTypeBuilder<ClientLog> builder )
    {
        builder.ToTable( name: nameof( AppDbContext.ClientLogs ), Schemas.APP );

        builder.Property( c => c.Id ).
            HasColumnType( "bigint" ).
            HasDefaultValueSql( $"nextval('{Schemas.APP}.sq__client_logs')" ).
            IsRequired();

        builder.Property( c => c.DeviceModelName ).
            HasMaxLength( 100 ).
            IsRequired( false );
        builder.Property( c => c.LogMessage ).
            IsRequired();
        builder.Property( c => c.LogType ).
            HasMaxLength( 100 ).
            IsRequired();
        builder.Property( c => c.AppVersion ).
            HasMaxLength( 10 ).
            IsRequired();
        builder.Property( c => c.DeviceManufacturer ).
            HasMaxLength( 100 ).
            IsRequired();
        builder.Property( c => c.DeviceOs ).
            HasMaxLength( 100 ).
            IsRequired();
        builder.Property( c => c.DeviceType ).
            HasMaxLength( 100 ).
            IsRequired();

        builder.Property(c => c.CreatedAt).
            HasDefaultValueSql( "now() at time zone 'Europe/Kiev'" ).
            IsRequired();
    }
}
