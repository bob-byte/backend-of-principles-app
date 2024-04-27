using BusinessLogic;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
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
    private readonly IProgressOfHabitService m_progressOfHabitService;
    private readonly IServiceOfHabit m_serviceOfHabit;

    public HabitController( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        m_progressOfHabitService = serviceProvider.GetRequiredService<IProgressOfHabitService>();
        m_serviceOfHabit = serviceProvider.GetRequiredService<IServiceOfHabit>();
    }

    [HttpGet( template: "inprogress/{userId}" )]
    public Task<IActionResult> InProgressIndex( Guid userId )
    {
        return TryCatchAsync( userId, async ( user ) =>
        {
            List<UserHabit> listOfHabits = await DbContext.UserHabits.
                Where( u => u.Status == StatusOfHabit.InProgress && u.UserId == userId ).
                Include( u => u.Progresses ).
                Include( u => u.Frequency ).
                OrderBy( u => u.Priority ).
                ToListAsync();

            List<UserHabitInProgressShortDto> resultData = new( listOfHabits.Count );
            for (int numPrc = 0; numPrc < listOfHabits.Count; numPrc++)
            {
                UserHabit habit = listOfHabits[numPrc];

                UserHabitInProgressShortDto dtoOfHabit = Mapper.Map<UserHabitInProgressShortDto>( habit );
                List<UserAreaOfLife> areasOfLife = await DbContext.
                    UserAreasOfLife.
                    Include( a => a.Habits ).
                    Where( a => a.Habits.Any( ua => ua.HabitId == habit.Id ) ).
                    ToListAsync();
                dtoOfHabit.AreasOfLife = areasOfLife;

                resultData.Add(dtoOfHabit);
            }

            OkObjectResult result = Ok( resultData );
            return result;
        } );
    }

    [HttpGet(template: "{habitId}")]
    public Task<IActionResult> Load( Guid habitId )
    {
        return TryCatchAsync( async () =>
        {
            UserHabit habit = await DbContext.UserHabits.
                Where( u => u.Id == habitId ).
                Include( u => u.AreasOfLife ).
                Include( u => u.Frequency ).
                FirstOrDefaultAsync();

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
                    ToListAsync();

                EditUserHabitDto resultData = Mapper.Map<EditUserHabitDto>( habit );
                resultData.AreasOfLife = areasOfLife;
                result = Ok( resultData );
            }

            return result;
        } );
    }

    [HttpPut( template: "{habitId}" )]
    public Task<IActionResult> Update( [FromBody] EditUserHabitDto habitDto, [FromQuery] Guid userId )
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
                ToListAsync();

            foreach (UserHabit userHabit in userHabitList.Where( h => h.Id != habitDto.Id ))
            {
                int? updatedPriority = habitDto.PrioritizedHabits.Find(h => h.Id == userHabit.Id)?.Priority;
                if(updatedPriority != null)
                {
                    userHabit.Priority = (int)updatedPriority;
                }
            }

            UserHabit? habit = habitDto.Id == Guid.Empty
                ? null
                : userHabitList.Find( h => h.Id == habitDto.Id );
            bool isNewHabit = habit == null;

            if(isNewHabit)
            {
                habit = Mapper.Map<UserHabit>( habitDto );
                if(habit.Id == Guid.Empty)
                {
                    habit.Id = Guid.NewGuid();
                }

                habit.FrequencyId = habitDto.Frequency.Id;
                habit.UserId = userId;
            }
            else
            {
                habit.Name = habitDto.Name;
                habit.FrequencyId = habitDto.Frequency.Id;
                habit.ReasonToFollow = habitDto.ReasonToFollow;
                habit.ColorName = habitDto.ColorName;
                habit.Description = habitDto.Description;
                habit.Question = habitDto.Question;
                habit.Type = habitDto.Type;
                habit.Priority = habitDto.Priority;
            }

            //TODO: don't use delete to reset areas of habit
            if (!isNewHabit)
            {
                await DbContext.UserAreasOfLifeUserHabits.Where( u => u.HabitId == habit.Id ).ExecuteDeleteAsync();
            }

            if (habitDto.AreasOfLife != null)
            {
                List<UserAreaOfLife> currentAreas = habitDto.AreasOfLife.ToList();
                foreach (UserAreaOfLife area in currentAreas)
                {
                    await DbContext.UserAreasOfLifeUserHabits.AddAsync( new UserAreaOfLifeUserHabit
                    {
                        HabitId = habit.Id,
                        AreaOfLifeId = area.Id
                    } );
                }
            }

            if (isNewHabit)
            {
                DbContext.UserHabits.AddOrUpdate( habit );
            }
            else
            {
                userHabitList.Remove( habit );
            }

            DbContext.UserHabits.UpdateRange( userHabitList );

            DbContext.Frequencies.AddOrUpdate( habitDto.Frequency );

            await DbContext.SaveChangesAsync();
            IActionResult actionResult = Ok();

            return actionResult;
        } );
    }

    [HttpPut("priorities/{userId}")]
    public Task<IActionResult> ResetPrioritiesAsync( Guid userId, [FromBody] List<UserHabitWithPriority> habits )
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

    [HttpPost]
    public Task<IActionResult> Post( [FromBody] UserHabit habit, [FromQuery] DateOnly startProgressInterval, [FromQuery] DateOnly endProgressInterval )
    {
        return TryCatchAsync( async () =>
        {
            bool isCorrectArg = habit != null;
            IActionResult actionResult;

            if (isCorrectArg)
            {
                DbContext.UserHabits.AddOrUpdate( habit );

                habit.Progresses ??= new List<ProgressOfHabit>();
                for (DateOnly date = startProgressInterval; date > endProgressInterval; date = date.AddDays( value: -1 ))
                {
                    if (!habit.Progresses.Any( p => p.Date == date ))
                    {
                        ProgressOfHabit progress = new()
                        {
                            Id = Guid.NewGuid(),
                            Date = date,
                            IsCompleted = false,
                            Habit = habit
                        };
                        DbContext.ProgressesOfHabits.AddOrUpdate( progress );
                        habit.Progresses.Add( progress );
                    }
                }

                await DbContext.SaveChangesAsync();
                actionResult = Ok( habit );
            }
            else
            {
                actionResult = BadRequest( error: "ProgressIsNull" );
            }

            return actionResult;
        } );
    }

    [HttpDelete("{habitId}")]
    public Task<IActionResult> Delete( Guid habitId )
    {
        return TryCatchAsync( async () =>
        {
            UserHabit? habit = habitId == Guid.Empty
                ? null
                : await DbContext.UserHabits.FindAsync( habitId );

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
                result = NotFound( habitId );
            }

            return result;
        } );
    }


    
}
