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
        TEntity[]? filteredTargetEntities,
        TDto[]? source,
        Func<TDto[], IEnumerable<TEntity>> getItemsToInsertInTarget,
        Action<TEntity, TDto>? updateExistingItem = null,
        string targetIdProp = "Id",
        string sourceIdProp = "Id"
    ) where TEntity : class
        where TDto : class
    {
        filteredTargetEntities ??= Array.Empty<TEntity>();
        source ??= Array.Empty<TDto>();
        
        //update database items that also are in client 
        if (updateExistingItem is not null && source.Length > 0)
        {
            foreach (TEntity targetItem in filteredTargetEntities)
            {
                TDto? sourceItem = source.FirstOrDefault( s =>
                    s.PropValue( targetIdProp )!.Equals( targetItem.PropValue( targetIdProp ) ) );
                if (sourceItem is not null)
                {
                    updateExistingItem( targetItem, sourceItem );
                }
            }
        }

        IEnumerable<TEntity> elemsNotFoundInSource = filteredTargetEntities.Where(
            t => !source.All( s => t.PropValue( targetIdProp )!.Equals( s.PropValue( sourceIdProp ) ) )
        );
        
        targetSet.RemoveRange( elemsNotFoundInSource );

        //insert new items that was added by client 
        TDto[] itemsThatNotExistInTarget = source
            .Where( s => !filteredTargetEntities.All( t => t.PropValue( targetIdProp )!.Equals( s.PropValue( sourceIdProp ) ) ) )
            .ToArray();

        IEnumerable<TEntity> toInsertItems = getItemsToInsertInTarget( itemsThatNotExistInTarget );
        await targetSet.AddRangeAsync( toInsertItems ).DefaultConfigureAwait();
    }

    public static void Merge<TEntity, TDto>(
        this DbSet<TEntity> targetSet,
        TEntity[]? filteredTargetEntities,
        TDto[]? source,
        Func<TDto[], IEnumerable<TEntity>> getItemsToInsertInTarget,
        Action<TEntity, TDto>? updateExistingItem = null,
        string targetIdProp = "Id",
        string sourceIdProp = "Id"
    ) where TEntity : class
        where TDto : class
    {
        filteredTargetEntities ??= Array.Empty<TEntity>();
        source ??= Array.Empty<TDto>();
        
        //update database items that also are in client 
        if (updateExistingItem is not null && source.Length > 0)
        {
            foreach (TEntity targetItem in filteredTargetEntities)
            {
                TDto? sourceItem = source.FirstOrDefault( s =>
                    s.PropValue( targetIdProp )!.Equals( targetItem.PropValue( targetIdProp ) ) );
                if (sourceItem is not null)
                {
                    updateExistingItem( targetItem, sourceItem );
                }
            }
        }

        //delete from database items that were removed by client 
        IEnumerable<TEntity> elemsNotFoundInSource = filteredTargetEntities.Where(
            t => !source.All( s => t.PropValue( targetIdProp )!.Equals( s.PropValue( sourceIdProp ) ) )
        );
        targetSet.RemoveRange( elemsNotFoundInSource );

        //insert new items that was added by client 
        TDto[] itemsThatNotExistInTarget = source
            .Where( s => !filteredTargetEntities.All( t => t.PropValue( targetIdProp )!.Equals( s.PropValue( sourceIdProp ) ) ) )
            .ToArray();

        IEnumerable<TEntity> toInsertItems = getItemsToInsertInTarget( itemsThatNotExistInTarget );
        targetSet.AddRange( toInsertItems );
    }
}
