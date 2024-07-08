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
    public async Task<IActionResult> Index( [FromQuery] long userId )
    {
        return await TryCatchAsync( async () =>
        {
            var userGoals = await DbContext.UserGoals
                .Where( g => g.UserId == userId )
                .Select( g => new UserGoalDto
                {
                    Id = g.Id,
                    Name = g.Name
                } )
                .ToListAsync();

            return Ok( userGoals );
        } );
    }

    [HttpPost( "updateallgoals" )]
    public async Task UpdateAllGoalsOfUser( EditUserHabitDto habitDto )
    {
        IEnumerable<long> sourceGoalIds = habitDto.AllUserGoals.Select( g => g.Id );

        UserGoal[] targetUserGoals = DbContext.UserGoals
            .Where( ug => ug.UserId == habitDto.Id )
            .ToArray();

        // Delete user goals
        IEnumerable<UserGoal> goalNotFoundInSource = targetUserGoals
            .Where( tg => !sourceGoalIds.Contains( tg.Id ) );
        DbContext.UserGoals.RemoveRange( goalNotFoundInSource );

        // Add new user goals
        IEnumerable<UserGoal> newUserGoals = habitDto.AllUserGoals
            .Where( g => g.Id == 0 )
            .Select( g => new UserGoal
            {
                Name = g.Name,
                UserId = habitDto.Id
            } );
        await DbContext.UserGoals.AddRangeAsync( newUserGoals );

        foreach (var sourceGoal in habitDto.AllUserGoals)
        {
            var targetGoal = targetUserGoals.FirstOrDefault( tg => tg.Id == sourceGoal.Id );
            if (targetGoal != null && targetGoal.Name != sourceGoal.Name)
            {
                targetGoal.Name = sourceGoal.Name;
            }
        }
        await DbContext.SaveChangesAsync().DefaultConfigureAwait();
    }

    [HttpDelete( "{goalId}" )]
    public Task<IActionResult> Delete( long goalId )
    {
        return TryCatchAsync( async () =>
        {
            UserGoal? goal = goalId == 0
                ? null
                : await DbContext.UserGoals.FindAsync( goalId );

            bool isCorrectArg = goal != null;

            IActionResult result;
            if (isCorrectArg)
            {
                await DbContext.UserGoals.Where( p => p.Id == goalId ).ExecuteDeleteAsync();
          
                result = Ok();
            }
            else
            {
                result = NotFound( goalId );
            }

            return result;
        } );
    }

    [HttpPost]
    public async Task<IActionResult> Update( [FromBody] UserGoalDto userGoal, [FromQuery( Name = "UserId" )] long userId )
    {
        return await TryCatchAsync( async () =>
        {
            UserGoal? existingGoal = userGoal.Id == 0
                ? null
                : await DbContext.UserGoals.FindAsync( userGoal.Id );

            IActionResult result;
            if (existingGoal != null)
            {
                existingGoal.Name = userGoal.Name;

                DbContext.UserGoals.Update( existingGoal );
                await DbContext.SaveChangesAsync();

                result = Ok();
            }
            else
            {
                var newGoal = new UserGoal
                {
                    Name = userGoal.Name,
                    UserId = userId
                };

                await DbContext.UserGoals.AddAsync( newGoal );
                await DbContext.SaveChangesAsync();
                result = Ok();
            }

            return result;
        } );
    }
}
