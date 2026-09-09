using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace SET.WebAPI.Controllers;

[Route( template: "api/goals" )]
[ApiController]
[Authorize( AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme )]
public class GoalController : BaseController
{
    public GoalController( IServiceProvider serviceProvider )
    : base( serviceProvider )
    {
        //do nothing
    }

    [HttpGet]
    public Task<IActionResult> Index( )
    {
        return TryCatchAsync( async (user) =>
        {
            List<UserGoalDto> userGoals = await DbContext.UserGoals.
                Where( g => g.UserId == user.Id ).
                Select( g => new UserGoalDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    IsCompleted = g.IsCompleted,
                    LastModified = g.UpdatedAt ?? g.CreatedAt
                } ).
                ToListAsync().
                DefaultConfigureAwait();

            return Ok( userGoals );
        } );
    }

    [HttpDelete( "{goalId}" )]
    public Task<IActionResult> Delete( long goalId )
    {
        return TryCatchAsync( async () =>
        {
            #region Check parameter
            if (goalId == 0)
            {
                return BadRequest( "GoalIdIsZero" );
            }
            #endregion

            UserGoal? goal = goalId == 0
                ? null
                : await DbContext.UserGoals.FindAsync( goalId ).DefaultConfigureAwait();

            bool isCorrectArg = goal != null;

            IActionResult result;
            if (isCorrectArg)
            {
                await using IDbContextTransaction tran = await DbContext.Database.BeginTransactionAsync().DefaultConfigureAwait();

                try
                {
                    await DbContext.UserHabits.Where( u => u.GoalId == goalId )
                        .ExecuteUpdateAsync(
                            setPropDelegate => setPropDelegate
                                .SetProperty( c => c.GoalId, c => null )
                                .SetProperty( c => c.UpdatedAt, DateTime.UtcNow ) )
                        .DefaultConfigureAwait();

                    DbContext.SyncDeletions.Add( new SyncDeletion
                    {
                        UserId = goal!.UserId,
                        EntityType = SyncEntityTypes.Goal,
                        EntityId = goalId,
                        DeletedAt = DateTime.UtcNow,
                    } );
                    await DbContext.SaveChangesAsync().DefaultConfigureAwait();

                    await DbContext.UserGoals.Where( p => p.Id == goalId ).ExecuteDeleteAsync().DefaultConfigureAwait();
                    await tran.CommitAsync().DefaultConfigureAwait();
                }
                catch
                {
                    await tran.RollbackAsync().DefaultConfigureAwait();
                    throw;
                }

                result = Ok();
            }
            else
            {
                result = BadRequest( "GoalIsNotFound" );
            }

            return result;
        } );
    }

    [HttpPost(template: "{goalId}")]
    public Task<IActionResult> Save( [FromBody] UserGoalDto userGoal)
    {
        return TryCatchAsync( async (User user) =>
        {
            #region Check parameter
            if(userGoal is null)
            {
                return BadRequest( "UserGoalDtoIsNull" );
            }
            #endregion

            UserGoal? existingGoal = userGoal.Id == 0
                ? null
                : await DbContext.UserGoals.FindAsync( userGoal.Id ).DefaultConfigureAwait();

            if (existingGoal is null && userGoal.Id != 0)
            {
                return BadRequest( $"GoalIsNotFoundWithId {userGoal.Id}" );
            }

            DtoWithId response = new();
            if (existingGoal is null)
            {
                var newGoal = new UserGoal
                {
                    Name = userGoal.Name,
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow,
                    IsCompleted = userGoal.IsCompleted
                };

                await DbContext.UserGoals.AddAsync( newGoal ).DefaultConfigureAwait();
                await DbContext.SaveChangesAsync().DefaultConfigureAwait();

                response.Id = newGoal.Id;
            }
            else
            {
                List<UserHabitReminder> remindersToUpdate = await DbContext.UserHabitReminders.
                    Where( h => h.Title == existingGoal.Name && existingGoal.UserId == user.Id ).
                    ToListAsync().
                    DefaultConfigureAwait();

                if (remindersToUpdate?.Count > 0)
                {
                    foreach (UserHabitReminder reminder in remindersToUpdate)
                    {
                        reminder.Title = userGoal.Name;
                    }
                    
                    DbContext.UserHabitReminders.UpdateRange( remindersToUpdate );
                }

                existingGoal.Name = userGoal.Name;
                existingGoal.IsCompleted = userGoal.IsCompleted;
                existingGoal.UpdatedAt = DateTime.UtcNow;

                DbContext.UserGoals.Update( existingGoal );
                await DbContext.SaveChangesAsync().DefaultConfigureAwait();

                response.Id = existingGoal.Id;
            }

            IActionResult result = Ok( response );
            return result;
        } );
    }
}
