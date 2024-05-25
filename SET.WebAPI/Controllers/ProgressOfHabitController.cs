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

            if (progressDto.HabitId == default)
            {
                return BadRequest( error: $"HabitId of ${progressDto.GetType().Name} is not set" );
            }
            #endregion

            bool isNewProgress = progressDto.Id == 0;

            ProgressOfHabit progress = isNewProgress
                ? Mapper.Map<ProgressOfHabit>( progressDto )
                : await DbContext.ProgressesOfHabits.FindAsync( progressDto.Id ).DefaultConfigureAwait();
            if(!isNewProgress)
            {
                progress.Value = progressDto.Value;
            }
            
            await DbContext.ProgressesOfHabits.AddOrUpdateAsync( progress ).DefaultConfigureAwait();
            await DbContext.SaveChangesAsync().DefaultConfigureAwait();

            var result = new
            {
                progressDto.Id
            };
            return Ok( result );
        } );
    }
}
