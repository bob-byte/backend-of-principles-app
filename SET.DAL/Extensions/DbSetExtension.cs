using Microsoft.EntityFrameworkCore;

using SET.Shared.Extensions;

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

    public static async Task MergeAsync<TEntity, TDto, TKey>(
        this DbSet<TEntity> targetSet,
        IEnumerable<TEntity> filteredTargetEntities,
        IEnumerable<TDto> source,
        Func<List<TDto>, IEnumerable<TEntity>> getItemsToInsertInTarget,
        Func<TEntity, TKey> targetKeySelector,
        Func<TDto, TKey> sourceKeySelector,
        Action<TEntity, TDto>? updateExistingItem = null
    )
        where TEntity : class
        where TDto : class
    {
        List<TEntity> targetList = filteredTargetEntities?.ToList() ?? new List<TEntity>();
        List<TDto> sourceList = source?.ToList() ?? new List<TDto>();

        // --- UPDATE EXISTING ITEMS ---
        if (updateExistingItem != null)
        {
            foreach (TEntity targetItem in targetList)
            {
                TKey targetKey = targetKeySelector( targetItem );

                // Match by key (skip default keys)
                TDto sourceItem = sourceList.FirstOrDefault(
                    s => EqualityComparer<TKey>.Default.Equals( sourceKeySelector( s ), targetKey )
                );

                if (sourceItem is not null)
                {
                    updateExistingItem( targetItem, sourceItem );
                }
            }
        }

        // --- DELETE REMOVED ITEMS ---
        // (those in target but not in source, skipping default keys like 0)
        List<TEntity> toRemove = targetList
            .Where( t =>
            {
                TKey targetKey = targetKeySelector( t );
                if (EqualityComparer<TKey>.Default.Equals( targetKey, default! ))
                {
                    return false; // ignore not-yet-saved entities
                }

                return !sourceList.Any(
                    s => EqualityComparer<TKey>.Default.Equals( sourceKeySelector( s ), targetKey ) );
            } )
            .ToList();

        if (toRemove.Any())
        {
            targetSet.RemoveRange( toRemove );
        }

        // --- INSERT NEW ITEMS ---
        List<TDto> toInsertDtos = sourceList
            .Where( s => EqualityComparer<TKey>.Default.Equals( sourceKeySelector( s ), default! ) ) // only new items
            .ToList();

        if (toInsertDtos.Count > 0)
        {
            IEnumerable<TEntity> toInsertEntities = getItemsToInsertInTarget( toInsertDtos );
            await targetSet.AddRangeAsync( toInsertEntities ).ConfigureAwait( false );
        }
    }

    public static void Merge<TEntity, TDto, TKey>(
        this DbSet<TEntity> targetSet,
        IEnumerable<TEntity> filteredTargetEntities,
        IEnumerable<TDto> source,
        Func<List<TDto>, IEnumerable<TEntity>> getItemsToInsertInTarget,
        Func<TEntity, TKey> targetKeySelector,
        Func<TDto, TKey> sourceKeySelector,
        Action<TEntity, TDto>? updateExistingItem = null
    )
        where TEntity : class
        where TDto : class
    {
        List<TEntity> targetList = filteredTargetEntities?.ToList() ?? new List<TEntity>();
        List<TDto> sourceList = source?.ToList() ?? new List<TDto>();

        // --- UPDATE EXISTING ITEMS ---
        if (updateExistingItem != null)
        {
            foreach (TEntity targetItem in targetList)
            {
                TKey targetKey = targetKeySelector( targetItem );

                // Match by key (skip default keys)
                TDto sourceItem = sourceList.FirstOrDefault(
                    s => EqualityComparer<TKey>.Default.Equals( sourceKeySelector( s ), targetKey )
                );

                if (sourceItem is not null)
                {
                    updateExistingItem( targetItem, sourceItem );
                }
            }
        }

        // --- DELETE REMOVED ITEMS ---
        // (those in target but not in source, skipping default keys like 0)
        List<TEntity> toRemove = targetList
            .Where( t =>
            {
                TKey targetKey = targetKeySelector( t );
                if (EqualityComparer<TKey>.Default.Equals( targetKey, default! ))
                {
                    return false; // ignore not-yet-saved entities
                }

                return !sourceList.Any(
                    s => EqualityComparer<TKey>.Default.Equals( sourceKeySelector( s ), targetKey ) );
            } )
            .ToList();

        if (toRemove.Any())
        {
            targetSet.RemoveRange( toRemove );
        }

        // --- INSERT NEW ITEMS ---
        List<TDto> toInsertDtos = sourceList
            .Where( s => EqualityComparer<TKey>.Default.Equals( sourceKeySelector( s ), default! ) ) // only new items
            .ToList();

        if (toInsertDtos.Count > 0)
        {
            IEnumerable<TEntity> toInsertEntities = getItemsToInsertInTarget( toInsertDtos );
            targetSet.AddRange( toInsertEntities );
        }
    }
}
