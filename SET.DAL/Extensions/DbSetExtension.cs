using Microsoft.EntityFrameworkCore;

using SET.Shared.Extensions;
using SET.Shared.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET.DataAccess.Extensions;

public static class DbSetExtension
{
    public static void AddOrUpdate<TEntity>(this DbSet<TEntity> entities, TEntity entity)
        where TEntity : EntityWithId
    {
        bool isAlreadyAdded = entities.Any( e => e.Id == entity.Id );
        if (isAlreadyAdded)
        {
            entities.Update( entity );
        }
        else
        {
            entities.Add( entity );
        }
    }
}
