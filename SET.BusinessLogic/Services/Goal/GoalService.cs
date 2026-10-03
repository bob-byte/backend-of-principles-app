using Microsoft.EntityFrameworkCore.Storage;

namespace BusinessLogic;

public class GoalService : IGoalService
{
    private readonly AppDbContext m_dbContext;
    private readonly ISyncPushService m_syncPushService;

    public GoalService( AppDbContext dbContext, ISyncPushService syncPushService )
    {
        m_dbContext = dbContext;
        m_syncPushService = syncPushService;
    }

    public Task<List<UserGoalDto>> GetActiveAsync( long userId )
    {
        return m_dbContext.UserGoals.
            Where( g => g.UserId == userId && !g.IsArchived ).
            Select( g => new UserGoalDto
            {
                Id = g.Id,
                Name = g.Name,
                Notes = g.Notes,
                IsCompleted = g.IsCompleted,
                IsArchived = g.IsArchived,
                LastModified = g.UpdatedAt ?? g.CreatedAt
            } ).
            ToListAsync();
    }

    public Task<List<ArchivedGoalResponse>> GetArchivedAsync( long userId )
    {
        return m_dbContext.UserGoals
            .Where( g => g.IsArchived && g.UserId == userId )
            .Select( g => new ArchivedGoalResponse
            {
                Id = g.Id,
                Name = g.Name,
                IsCompleted = g.IsCompleted,
                LastModified = g.UpdatedAt ?? g.ArchivingTime ?? g.CreatedAt
            } )
            .OrderByDescending( g => g.Id )
            .ToListAsync();
    }

    public async Task<ServiceResult> SetArchiveStatusAsync( long userId, GoalArchiveStatus goalArchiveStatus, string? originDeviceId )
    {
        #region Check parameter
        if (goalArchiveStatus is null)
        {
            return ServiceError.BadRequest( "GoalArchiveStatusIsNull" );
        }

        if (goalArchiveStatus.GoalId == 0)
        {
            return ServiceError.BadRequest( "GoalIdIsZero" );
        }
        #endregion

        UserGoal? goal = await m_dbContext.UserGoals
            .Where( g => g.Id == goalArchiveStatus.GoalId && g.UserId == userId )
            .FirstOrDefaultAsync()
            .DefaultConfigureAwait();

        if (goal is null)
        {
            return ServiceError.BadRequest( "GoalIsNotFound" );
        }

        goal.IsArchived = goalArchiveStatus.IsArchived;
        goal.UpdatedAt = DateTime.UtcNow;
        goal.ArchivingTime = goal.IsArchived ? goal.UpdatedAt : null;

        await m_dbContext.SaveChangesAsync().DefaultConfigureAwait();
        m_syncPushService.NotifyOtherDevices( goal.UserId, originDeviceId );

        return ServiceResult.Success;
    }

    public async Task<ServiceResult> DeleteAsync( long userId, long goalId )
    {
        #region Check parameter
        if (goalId == 0)
        {
            return ServiceError.BadRequest( "GoalIdIsZero" );
        }
        #endregion

        UserGoal? goal = await m_dbContext.UserGoals
            .FirstOrDefaultAsync( g => g.Id == goalId && g.UserId == userId )
            .DefaultConfigureAwait();
        if (goal is null)
        {
            return ServiceError.BadRequest( "GoalIsNotFound" );
        }

        await using IDbContextTransaction tran = await m_dbContext.Database.BeginTransactionAsync().DefaultConfigureAwait();

        try
        {
            await m_dbContext.UserHabits.Where( u => u.GoalId == goalId && u.UserId == userId )
                .ExecuteUpdateAsync(
                    setPropDelegate => setPropDelegate
                        .SetProperty( c => c.GoalId, c => null )
                        .SetProperty( c => c.UpdatedAt, DateTime.UtcNow ) )
                .DefaultConfigureAwait();

            m_dbContext.SyncDeletions.Add( new SyncDeletion
            {
                UserId = goal.UserId,
                EntityType = SyncEntityTypes.Goal,
                EntityId = goalId,
                DeletedAt = DateTime.UtcNow,
            } );
            await m_dbContext.SaveChangesAsync().DefaultConfigureAwait();

            await m_dbContext.UserGoals.Where( p => p.Id == goalId ).ExecuteDeleteAsync().DefaultConfigureAwait();
            await tran.CommitAsync().DefaultConfigureAwait();
        }
        catch
        {
            await tran.RollbackAsync().DefaultConfigureAwait();
            throw;
        }

        return ServiceResult.Success;
    }

    public async Task<ServiceResult<DtoWithId>> SaveAsync( User user, UserGoalDto userGoal )
    {
        #region Check parameter
        if (userGoal is null)
        {
            return ServiceError.BadRequest( "UserGoalDtoIsNull" );
        }
        #endregion

        UserGoal? existingGoal = userGoal.Id == 0
            ? null
            : await m_dbContext.UserGoals
                .FirstOrDefaultAsync( g => g.Id == userGoal.Id && g.UserId == user.Id )
                .DefaultConfigureAwait();

        if (existingGoal is null && userGoal.Id != 0)
        {
            return ServiceError.BadRequest( $"GoalIsNotFoundWithId {userGoal.Id}" );
        }

        DtoWithId response = new();
        if (existingGoal is null)
        {
            var newGoal = new UserGoal
            {
                Name = userGoal.Name,
                Notes = string.IsNullOrWhiteSpace( userGoal.Notes ) ? null : userGoal.Notes.Trim(),
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                IsCompleted = userGoal.IsCompleted,
                IsArchived = userGoal.IsArchived,
                ArchivingTime = userGoal.IsArchived ? DateTime.UtcNow : null,
            };

            await m_dbContext.UserGoals.AddAsync( newGoal ).DefaultConfigureAwait();
            await m_dbContext.SaveChangesAsync().DefaultConfigureAwait();

            response.Id = newGoal.Id;
        }
        else
        {
            List<UserHabitReminder> remindersToUpdate = await m_dbContext.UserHabitReminders.
                Where( h => h.Title == existingGoal.Name && h.UserHabit.UserId == user.Id ).
                ToListAsync().
                DefaultConfigureAwait();

            if (remindersToUpdate?.Count > 0)
            {
                foreach (UserHabitReminder reminder in remindersToUpdate)
                {
                    reminder.Title = userGoal.Name;
                }

                m_dbContext.UserHabitReminders.UpdateRange( remindersToUpdate );
            }

            existingGoal.Name = userGoal.Name;
            existingGoal.Notes = string.IsNullOrWhiteSpace( userGoal.Notes ) ? null : userGoal.Notes.Trim();
            existingGoal.IsCompleted = userGoal.IsCompleted;
            existingGoal.IsArchived = userGoal.IsArchived;
            existingGoal.UpdatedAt = DateTime.UtcNow;
            existingGoal.ArchivingTime = existingGoal.IsArchived ? existingGoal.UpdatedAt : null;

            m_dbContext.UserGoals.Update( existingGoal );
            await m_dbContext.SaveChangesAsync().DefaultConfigureAwait();

            response.Id = existingGoal.Id;
        }

        return response;
    }
}
