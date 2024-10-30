using Microsoft.EntityFrameworkCore;

using SET.Shared.Extensions;

using System.Threading.Tasks;

namespace SET.DataAccess.Extensions;

public static class DbSetExtension
{
    public static void AddOrUpdate<TEntity>( this DbSet<TEntity> entities, TEntity entity )
        where TEntity : class
    {
        if (entity.PropValue<long>( "Id" ) == 0)
        {
            entities.Add( entity );
        }
        else
        {
            entities.Update( entity );
        }
    }

    public static async ValueTask AddOrUpdateAsync<TEntity>( this DbSet<TEntity> entities, TEntity entity )
        where TEntity : class
    {
        if (entity.PropValue<long>( "Id" ) == 0)
        {
            await entities.AddAsync( entity ).DefaultConfigureAwait();
        }
        else
        {
            entities.Update( entity );
        }
    }
}
