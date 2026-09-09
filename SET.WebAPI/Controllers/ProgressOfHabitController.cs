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
        return TryCatchAsync( async ( User user ) =>
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

            bool habitExists = await DbContext.UserHabits
                .AnyAsync( h => h.Id == progressDto.HabitId && h.UserId == user.Id )
                .DefaultConfigureAwait();
            if (!habitExists)
            {
                return BadRequest( error: $"HabitIsNotFoundWithId {progressDto.HabitId}" );
            }

            ProgressOfHabit? progress =
                await DbContext.ProgressesOfHabits.FirstOrDefaultAsync( p =>
                    p.HabitId == progressDto.HabitId && p.Date == progressDto.Date ).DefaultConfigureAwait();
            bool isNewProgress = progress is null;
            
            if (isNewProgress)
            {
                progress = Mapper.Map<ProgressOfHabit>( progressDto );
                progress.Id = 0;
            }
            else
            {
                progress.Value = progressDto.Value;
            }
            
            await DbContext.ProgressesOfHabits.AddOrUpdateAsync( progress ).DefaultConfigureAwait();
            await DbContext.UserHabits
                .Where( h => h.Id == progressDto.HabitId && h.UserId == user.Id )
                .ExecuteUpdateAsync( s => s.SetProperty( h => h.UpdatedAt, DateTime.UtcNow ) )
                .DefaultConfigureAwait();
            await DbContext.SaveChangesAsync().DefaultConfigureAwait();

            var result = new
            {
                progress.Id
            };
            return Ok( result );
        } );
    }
}
