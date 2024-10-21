using Microsoft.EntityFrameworkCore;

using SET.Shared.Extensions;
using SET.Shared.Models;

using System.Collections.Generic;
using System.Linq;
using System;
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

    public static async Task MergeAsync<TEntity, TDto>(
        this DbSet<TEntity> targetSet,
        TEntity[] filteredTargetEntities,
        IEnumerable<TDto> source,
        Func<TDto[], IEnumerable<TEntity>> getItemsToInsertInTarget,
        string targetIdProp = "Id",
        string sourceIdProp = "Id"
    ) where TEntity : class
      where TDto : class
    {
        //delete from database items that were removed by client 
        IEnumerable<TEntity> elemsNotFoundInSource = filteredTargetEntities.Where( t => !source.Any( s => s.PropValue( sourceIdProp ) == t.PropValue( targetIdProp ) ) );
        targetSet.RemoveRange( elemsNotFoundInSource );

        //insert new items that was added by client 
        TDto[] itemsThatNotExistInTarget = source.Where( s => !filteredTargetEntities.Any( t => t.PropValue( targetIdProp ) == s.PropValue( sourceIdProp ) ) ).ToArray();

        IEnumerable<TEntity> toInsertItems = getItemsToInsertInTarget( itemsThatNotExistInTarget );
        await targetSet.AddRangeAsync( toInsertItems ).DefaultConfigureAwait();
    }

    public static void Merge<TEntity, TDto>(
        this DbSet<TEntity> targetSet,
        TEntity[] filteredTargetEntities,
        IEnumerable<TDto> source,
        Func<TDto[], IEnumerable<TEntity>> getItemsToInsertInTarget,
        string targetIdProp = "Id",
        string sourceIdProp = "Id"
    ) where TEntity : class
      where TDto : class
    {
        //delete from database items that were removed by client 
        IEnumerable<TEntity> elemsNotFoundInSource = filteredTargetEntities.Where( t => !source.Any( s => s.PropValue( sourceIdProp ) == t.PropValue( targetIdProp ) ) );
        targetSet.RemoveRange( elemsNotFoundInSource );

        //insert new items that was added by client 
        TDto[] itemsThatNotExistInTarget = source.Where( s => !filteredTargetEntities.Any( t => t.PropValue( targetIdProp ) == s.PropValue( sourceIdProp ) ) ).ToArray();

        IEnumerable<TEntity> toInsertItems = getItemsToInsertInTarget( itemsThatNotExistInTarget );
        targetSet.AddRange( toInsertItems );
    }
}
