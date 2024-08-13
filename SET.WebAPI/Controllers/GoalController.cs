using Microsoft.EntityFrameworkCore;

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
    public Task<IActionResult> Index( [FromQuery] long userId )
    {
        return TryCatchAsync( async () =>
        {
            List<UserGoalDto> userGoals = await DbContext.UserGoals.
                Where( g => g.UserId == userId ).
                Select( g => new UserGoalDto
                {
                    Id = g.Id,
                    Name = g.Name
                } ).
                OrderBy( g => g.Id ).
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
                await DbContext.
                    UserHabits.
                    Where( u => u.GoalId == goalId ).
                    ExecuteUpdateAsync( setPropDelegate => setPropDelegate.SetProperty( c => c.GoalId, c => null ) ).
                    DefaultConfigureAwait();

                await DbContext.
                    UserGoals.
                    Where( p => p.Id == goalId ).
                    ExecuteDeleteAsync().
                    DefaultConfigureAwait();
          
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
    public Task<IActionResult> Update( [FromBody] UserGoalDto userGoal, [FromQuery( Name = "userId" )] long userId )
    {
        return TryCatchAsync( userId, async (User _) =>
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
                    UserId = userId
                };

                await DbContext.UserGoals.AddAsync( newGoal ).DefaultConfigureAwait();
                await DbContext.SaveChangesAsync().DefaultConfigureAwait();

                response.Id = newGoal.Id;
            }
            else
            {
                existingGoal.Name = userGoal.Name;

                DbContext.UserGoals.Update( existingGoal );
                await DbContext.SaveChangesAsync().DefaultConfigureAwait();

                response.Id = existingGoal.Id;
            }

            IActionResult result = Ok( response );
            return result;
        } );
    }
}
