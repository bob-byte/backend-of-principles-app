using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SET.Shared.Extensions;
using SET.WebAPI.Helpers;
using SET.WebAPI.Models;
using TaskEntity = SET.Shared.Models.Task;

namespace SET.WebAPI.Controllers;

[Route("api/tasks")]
[ApiController]
[Authorize( AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme )]
public class TasksController : BaseController
{
    public TasksController( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
    }

    [HttpPost]
    public Task<IActionResult> CreateAsync( [FromBody] TaskItemDto request )
    {
        return TryCatchAsync( async user =>
        {
            if (request is null)
            {
                return BadRequest( "TaskIsNull" );
            }

            if (string.IsNullOrWhiteSpace( request.Name ))
            {
                return BadRequest( "NameIsNullOrWhiteSpace" );
            }

            if (request.Name.Length > 255)
            {
                return BadRequest( "NameIsTooLong" );
            }

            TaskEntity entity = new()
            {
                UserId = user.Id,
                IsCompleted = request.IsCompleted
            };
            TaskDtoMapper.ApplyDto( entity, request );

            await DbContext.Tasks.AddAsync( entity ).DefaultConfigureAwait();
            await DbContext.SaveChangesAsync().DefaultConfigureAwait();

            return Created( $"api/tasks/{entity.Id}", TaskDtoMapper.ToDto( entity ) );
        } );
    }

    [HttpGet]
    public Task<IActionResult> GetByDateAsync( [FromQuery] DateOnly? date )
    {
        return TryCatchAsync( async user =>
        {
            if (date is null)
            {
                return BadRequest( "DateIsNotSpecified" );
            }

            List<TaskEntity> entities = await DbContext.Tasks
                .Include( t => t.Subtasks )
                .Where( t => t.UserId == user.Id && t.Date == date )
                .OrderBy( t => t.Time )
                .ToListAsync()
                .DefaultConfigureAwait();

            return Ok( entities.Select( TaskDtoMapper.ToDto ).ToList() );
        } );
    }

    [HttpGet("all")]
    public Task<IActionResult> GetAllAsync()
    {
        return TryCatchAsync( async user =>
        {
            List<TaskEntity> entities = await DbContext.Tasks
                .Include( t => t.Subtasks )
                .Where( t => t.UserId == user.Id )
                .OrderBy( t => t.Date )
                .ThenBy( t => t.Time )
                .ToListAsync()
                .DefaultConfigureAwait();

            return Ok( entities.Select( TaskDtoMapper.ToDto ).ToList() );
        } );
    }

    [HttpPut( "{id:long}/status" )]
    public Task<IActionResult> UpdateStatusAsync( long id, [FromBody] UpdateTaskStatusDto request )
    {
        return TryCatchAsync( async user =>
        {
            if (id <= 0)
            {
                return BadRequest( "TaskIdIsZeroOrNegative" );
            }

            if (request is null)
            {
                return BadRequest( "RequestIsNull" );
            }

            TaskEntity? entity = await DbContext.Tasks
                .Include( t => t.Subtasks )
                .FirstOrDefaultAsync( t => t.Id == id && t.UserId == user.Id )
                .DefaultConfigureAwait();

            if (entity is null)
            {
                return NotFound( $"TaskIsNotFoundWithId {id}" );
            }

            entity.IsCompleted = request.IsCompleted;

            await DbContext.SaveChangesAsync().DefaultConfigureAwait();

            return Ok( TaskDtoMapper.ToDto( entity ) );
        } );
    }


    [HttpPut( "{id:long}" )]
    public Task<IActionResult> UpdateAsync( long id, [FromBody] TaskItemDto request )
    {
        return TryCatchAsync( async user =>
        {
            if (id <= 0) return BadRequest( "TaskIdIsZeroOrNegative" );
            if (request is null) return BadRequest( "RequestIsNull" );
            if (string.IsNullOrWhiteSpace( request.Name )) return BadRequest( "NameIsNullOrWhiteSpace" );

            TaskEntity? entity = await DbContext.Tasks
                .Include( t => t.Subtasks )
                .FirstOrDefaultAsync( t => t.Id == id && t.UserId == user.Id )
                .DefaultConfigureAwait();

            if (entity is null) return NotFound( $"TaskIsNotFoundWithId {id}" );

            TaskDtoMapper.ApplyDto( entity, request );

            await DbContext.SaveChangesAsync().DefaultConfigureAwait();

            return Ok( TaskDtoMapper.ToDto( entity ) );
        } );
    }

    [HttpDelete( "{id:long}" )]
    public Task<IActionResult> DeleteAsync( long id )
    {
        return TryCatchAsync( async user =>
        {
            if (id <= 0)
            {
                return BadRequest( "TaskIdIsZeroOrNegative" );
            }

            TaskEntity? entity = await DbContext.Tasks
                .FirstOrDefaultAsync( t => t.Id == id && t.UserId == user.Id )
                .DefaultConfigureAwait();

            if (entity is null)
            {
                return NotFound( $"TaskIsNotFoundWithId {id}" );
            }

            DbContext.Tasks.Remove( entity );
            await DbContext.SaveChangesAsync().DefaultConfigureAwait();

            return NoContent();
        } );
    }

    [HttpGet("inbox")]
    public Task<IActionResult> GetInboxTasksAsync()
    {
        return TryCatchAsync( async user =>
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            List<TaskEntity> entities = await DbContext.Tasks
                .Include( t => t.Subtasks )
                .Where( t => t.UserId == user.Id
                          && t.IsCompleted == false
                          && (t.Date == null || t.Date >= today) )
                .OrderBy( t => t.Date )
                .ThenBy( t => t.Time )
                .ToListAsync()
                .DefaultConfigureAwait();

            return Ok( entities.Select( TaskDtoMapper.ToDto ).ToList() );
        } );
    }
}
