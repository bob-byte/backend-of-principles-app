using BusinessLogic;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using SET.Shared;
using SET.Shared.Models;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Linq;
using System.Text.Json;

namespace SET.WebAPI.Controllers;

[Route( template: "api/habits")]
[ApiController]
[Authorize( AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme )]
public class HabitController : BaseController
{
    public HabitController( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        //do nothing
    }

    [HttpGet( template: "inprogress/{userId}" )]
    public Task<IActionResult> InProgressIndex( long userId )
    {
        return TryCatchAsync( userId, async ( user ) =>
        {
            List<UserHabit> listOfHabits = await DbContext.UserHabits.
                Where( u => u.Status == StatusOfHabit.InProgress && u.UserId == userId ).
                Include( u => u.Progresses ).
                Include( u => u.Frequency ).
                Include( u => u.AreasOfLife ).
                Include( u => u.Goal ).
                OrderBy( u => u.Priority ).
                AsSplitQuery().
                ToListAsync().
                DefaultConfigureAwait();

            List<UserHabitInProgressShortDto> resultData = Mapper.Map<List<UserHabitInProgressShortDto>>( listOfHabits );
            OkObjectResult result = Ok( resultData );
            return result;
        } );
    }

    [HttpGet(template: "{habitId}")]
    public Task<IActionResult> Load( long habitId )
    {
        return TryCatchAsync( async () =>
        {
            UserHabit habit = await DbContext.UserHabits.
                Where( u => u.Id == habitId ).
                Include( u => u.Frequency ).
                Include( u => u.Goal ).
                FirstOrDefaultAsync().
                DefaultConfigureAwait();

            IActionResult result;
            if(habit == null)
            {
                result = BadRequest( $"{nameof(UserHabit)} is not found" );
            }
            else
            {
                List<UserAreaOfLife> areasOfLife = await DbContext.
                    UserAreasOfLife.
                    Include( u => u.Habits ).
                    Where( u => u.Habits.Any( up => up.HabitId == habitId )).
                    ToListAsync().
                    DefaultConfigureAwait();

                EditUserHabitDto resultData = Mapper.Map<EditUserHabitDto>( habit );
                resultData.AreasOfLife = Mapper.Map<List<UserAreaOfLifeDto>>( areasOfLife );
                result = Ok( resultData );
            }

            return result;
        } );
    }

    [HttpPost( template: "{habitId}" )]
    public Task<IActionResult> Update( [FromBody] EditUserHabitDto habitDto, [FromQuery] long userId )
    {
        return TryCatchAsync( userId, async (User user) =>
        {
            #region Check habitDto param
            bool isCorrectArg = habitDto != null;

            if(!isCorrectArg)
            {
                return BadRequest( "HabitIsNull" );
            }

            if (habitDto.Frequency == null)
            {
                return BadRequest( "FrequencyIsNull" );
            }
            #endregion

            List<UserHabit> userHabitList = await DbContext.
                UserHabits.
                Where( h => h.UserId == userId ).
                ToListAsync().
                DefaultConfigureAwait();

            foreach (UserHabit userHabit in userHabitList.Where( h => h.Id != habitDto.Id ))
            {
                int? updatedPriority = habitDto.PrioritizedHabits?.Find(h => h.Id == userHabit.Id )?.Priority;
                if(updatedPriority != null)
                {
                    userHabit.Priority = (int)updatedPriority;
                }
            }

            UserHabit? habit = habitDto.Id == 0
                ? null
                : userHabitList.Find( h => h.Id == habitDto.Id );
            bool isNewHabit = habit == null;

            if (isNewHabit)
            {
                habit = Mapper.Map<UserHabit>( habitDto );
                habit.UserId = userId;
            }
            else
            {
                habit.Name = habitDto.Name;
                habit.FrequencyId = habitDto.Frequency.Id;
                habit.Frequency = Mapper.Map<Frequency>(habitDto.Frequency);
                habit.ColorName = habitDto.ColorName;
                habit.ReasonToFollow = habitDto.ReasonToFollow;
                habit.Description = habitDto.Description;
                habit.Question = habitDto.Question;
                habit.Complexity = habitDto.Complexity;
                habit.Type = habitDto.Type;
                habit.Priority = habitDto.Priority;
            }

            if (isNewHabit)
            {
                if (habitDto.AreasOfLife?.Any() == true)
                {
                    List<UserAreaOfLifeDto> currentAreas = habitDto.AreasOfLife.ToList();
                    foreach (UserAreaOfLifeDto area in currentAreas)
                    {
                        await DbContext.UserAreasOfLifeUserHabits.AddAsync( new UserAreaOfLifeUserHabit
                        {
                            Habit = habit,
                            AreaOfLifeId = area.Id
                        } );
                    }
                }
            }
            else
            {
                //this block is an analog to MERGE operator which inserts and deletes

                IEnumerable<long> sourceAreasIds = habitDto.AreasOfLife.Select( s => s.Id );
                UserAreaOfLifeUserHabit[] targetAreasAndHabits = DbContext.UserAreasOfLifeUserHabits.
                    Where( u => u.HabitId == habit.Id ).
                    ToArray();

                //delete from database items that were removed by client
                IEnumerable<UserAreaOfLifeUserHabit> elemsNotFoundInSource = targetAreasAndHabits.Where( s => !sourceAreasIds.Contains( s.AreaOfLifeId ) );
                DbContext.UserAreasOfLifeUserHabits.RemoveRange( elemsNotFoundInSource );

                //insert new areas of life that was added by client
                long[] itemsThatNotExistInSource = sourceAreasIds.Where( sourceAreaId => !targetAreasAndHabits.Any( a => a.AreaOfLifeId == sourceAreaId ) ).ToArray();
                IEnumerable<UserAreaOfLifeUserHabit> toInsertAreas = itemsThatNotExistInSource.Select(
                    areaId => new UserAreaOfLifeUserHabit()
                    {
                        AreaOfLifeId = areaId,
                        HabitId = habit.Id
                    }
                );
                await DbContext.UserAreasOfLifeUserHabits.AddRangeAsync( toInsertAreas ).DefaultConfigureAwait();
            }

            if (isNewHabit)
            {
                await DbContext.UserHabits.AddOrUpdateAsync( habit ).DefaultConfigureAwait();
            }
            else
            {
                userHabitList.Remove( habit );
            }

            if (!isNewHabit)
            {
                habit.GoalId = habitDto.Goal.Id;
                habit.Goal = Mapper.Map<UserGoal>( habitDto.Goal );
            }

            await DbContext.Frequencies.AddOrUpdateAsync( habit.Frequency ).DefaultConfigureAwait();
            DbContext.UserHabits.UpdateRange( userHabitList );

            await DbContext.SaveChangesAsync().DefaultConfigureAwait();

            var result = new
            {
                habit.Id,
                habit.FrequencyId
            };

            return Ok( result );
        } );
    }

    [HttpPut("priorities/{userId}")]
    public Task<IActionResult> ResetPrioritiesAsync( long userId, [FromBody] List<UserHabitWithPriority> habits )
    {
        return TryCatchAsync( userId, async (user) =>
        {
            #region Check param
            if (habits == null || habits.Count < 2)
            {
                return BadRequest( error: "Habits with priorities are less than 2" );
            }
            #endregion

            List<UserHabit> userHabitList = await DbContext.
                UserHabits.
                Where( h => h.UserId == userId ).
                ToListAsync().
                DefaultConfigureAwait();

            IActionResult result;
            foreach (UserHabit userHabit in userHabitList)
            {
                int updatedPriority = habits.Find( h => h.Id == userHabit.Id )!.Priority;
                userHabit.Priority = updatedPriority;
            }

            await DbContext.SaveChangesAsync().DefaultConfigureAwait();
            result = Ok();

            return result;
        } );
    }

    [HttpDelete("{habitId}")]
    public Task<IActionResult> Delete( long habitId )
    {
        return TryCatchAsync( async () =>
        {
            #region Check parameter
            if (habitId == 0)
            {
                return BadRequest( "HabitIdIsZero" );
            }
            #endregion

            UserHabit? habit = habitId == 0
                ? null
                : await DbContext.UserHabits.FindAsync( habitId ).DefaultConfigureAwait();

            bool isCorrectArg = habit != null;

            IActionResult result;
            if (isCorrectArg)
            {
                await DbContext.ProgressesOfHabits.Where( p => p.HabitId == habitId ).ExecuteDeleteAsync();
                await DbContext.UserAreasOfLifeUserHabits.Where( p => p.HabitId == habitId ).ExecuteDeleteAsync();
                await DbContext.UserHabits.Where( u => u.Id == habitId ).ExecuteDeleteAsync();
                await DbContext.Frequencies.Where( f => f.Id == habit.FrequencyId ).ExecuteDeleteAsync();

                result = Ok();
            }
            else
            {
                result = BadRequest( $"HabitIsNotFoundWithId {habitId}" );
            }

            return result;
        } );
    }
}
