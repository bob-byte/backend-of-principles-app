using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using BusinessLogic;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

using SET.Shared.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SET.WebAPI.Controllers;

[Route( "api/progressesofhabit" )]
[ApiController]
[Authorize( AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme )]
public class ProgressOfHabitController : BaseController
{
    public ProgressOfHabitController( IServiceProvider serviceProvider ) : base( serviceProvider )
    {
        //do nothing
    }

    [HttpPost( template: "{progressId}" )]
    public Task<IActionResult> UpdateProgress( [FromBody] UpdateProgressDto progressDto )
    {
        return TryCatchAsync( async () =>
        {
            #region Check param
            if (progressDto == null)
            {
                return BadRequest( error: "ProgressIsNull" );
            }

            if (progressDto.Date == default)
            {
                return BadRequest( error: $"Date is {progressDto.Date}" );
            }
            #endregion

            UserHabit? habit = await DbContext.
                UserHabits.
                FirstOrDefaultAsync( p => p.Id == progressDto.Id );

            if(habit == null)
            {
                return BadRequest( "Habit of progress is not found" );
            }

            habit.PercentageAchieved = progressDto.PercentageAchieved;

            ProgressOfHabit progress = await DbContext.
                ProgressesOfHabits.
                FirstOrDefaultAsync( p => p.Id == progressDto.Id || (p.HabitId == habit.Id && p.Date == progressDto.Date) );
            if(progress == null)
            {
                progress = Mapper.Map<ProgressOfHabit>( progressDto );
            }
            else
            {
                progress.Value = progressDto.Value;
            }

            DbContext.ProgressesOfHabits.AddOrUpdate( progress );
            await DbContext.SaveChangesAsync();

            var result = new
            {
                HabitId = habit.Id,
                ProgressId = progress.Id
            };
            return Ok( result );
        } );
    }
}
