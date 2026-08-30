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

            if (request.Notes is not null && request.Notes.Length > 255)
            {
                return BadRequest( "NotesAreTooLong" );
            }

            TaskEntity entity = new()
            {
                UserId = user.Id,
                Name = request.Name,
                Notes = request.Notes,
                Date = request.Date,
                Time = request.Time,
                IsCompleted = request.IsCompleted
            };

            await DbContext.Tasks.AddAsync( entity ).DefaultConfigureAwait();
            await DbContext.SaveChangesAsync().DefaultConfigureAwait();

            TaskItemDto response = new()
            {
                Id = entity.Id,
                Name = entity.Name,
                Notes = entity.Notes,
                Date = entity.Date,
                Time = entity.Time,
                IsCompleted = entity.IsCompleted
            };

            return Created( $"api/tasks/{response.Id}", response );
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

            List<TaskItemDto> items = await DbContext.Tasks
                .Where( t => t.UserId == user.Id && t.Date == date )
                .OrderBy( t => t.Time )
                .Select( t => new TaskItemDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Notes = t.Notes,
                    Date = t.Date,
                    Time = t.Time,
                    IsCompleted = t.IsCompleted
                } )
                .ToListAsync()
                .DefaultConfigureAwait();

            return Ok( items );
        } );
    }

    [HttpGet("all")]
    public Task<IActionResult> GetAllAsync()
    {
        return TryCatchAsync( async user =>
        {
            List<TaskItemDto> items = await DbContext.Tasks
                .Where( t => t.UserId == user.Id )
                .OrderBy( t => t.Date ) 
                .ThenBy( t => t.Time ) 
                .Select( t => new TaskItemDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Notes = t.Notes,
                    Date = t.Date,
                    Time = t.Time,
                    IsCompleted = t.IsCompleted
                } )
                .ToListAsync()
                .DefaultConfigureAwait();

            return Ok( items );
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
                .FirstOrDefaultAsync( t => t.Id == id && t.UserId == user.Id )
                .DefaultConfigureAwait();

            if (entity is null)
            {
                return NotFound( $"TaskIsNotFoundWithId {id}" );
            }

            entity.IsCompleted = request.IsCompleted;

            await DbContext.SaveChangesAsync().DefaultConfigureAwait();

            TaskItemDto response = new()
            {
                Id = entity.Id,
                Name = entity.Name,
                Notes = entity.Notes,
                Date = entity.Date,
                Time = entity.Time,
                IsCompleted = entity.IsCompleted
            };

            return Ok( response );
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
            .FirstOrDefaultAsync( t => t.Id == id && t.UserId == user.Id )
            .DefaultConfigureAwait();

            if (entity is null) return NotFound( $"TaskIsNotFoundWithId {id}" );

            entity.Name = request.Name;
            entity.Notes = request.Notes;
            entity.Date = request.Date;
            entity.Time = request.Time;

            await DbContext.SaveChangesAsync().DefaultConfigureAwait();

            TaskItemDto response = new()
            {
                Id = entity.Id,
                Name = entity.Name,
                Notes = entity.Notes,
                Date = entity.Date,
                Time = entity.Time,
                IsCompleted = entity.IsCompleted
            };

            return Ok( response );
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

            List<TaskItemDto> items = await DbContext.Tasks
                .Where( t => t.UserId == user.Id 
                          && t.IsCompleted == false 
                          && (t.Date == null || t.Date >= today) )
                .OrderBy( t => t.Date ) 
                .ThenBy( t => t.Time )
                .Select( t => new TaskItemDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Notes = t.Notes,
                    Date = t.Date,
                    Time = t.Time,
                    IsCompleted = t.IsCompleted
                } )
                .ToListAsync()
                .DefaultConfigureAwait();

            return Ok( items );
        } );
    }
}

