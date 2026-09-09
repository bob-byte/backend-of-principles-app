using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SET.Shared.Models;
using SET.WebAPI.Helpers;
using SET.WebAPI.Models;

namespace SET.WebAPI.Controllers;

[Route( "api/ai/conversations" )]
[ApiController]
[Authorize( AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme )]
public class AiConversationsController : BaseController
{
    public AiConversationsController( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
    }

    [HttpGet]
    public Task<IActionResult> ListAsync()
    {
        return TryCatchAsync( async user =>
        {
            List<AiConversation> entities = await DbContext.AiConversations
                .AsNoTracking()
                .Include( c => c.Messages )
                .Where( c => c.UserId == user.Id )
                .OrderByDescending( c => c.UpdatedAt )
                .ToListAsync()
                .DefaultConfigureAwait();

            return Ok( entities.Select( c => AiConversationDtoMapper.ToDto( c ) ).ToList() );
        } );
    }

    [HttpGet( "{id:long}" )]
    public Task<IActionResult> GetByIdAsync( long id )
    {
        return TryCatchAsync( async user =>
        {
            if (id <= 0)
            {
                return BadRequest( "ConversationIdIsZeroOrNegative" );
            }

            AiConversation? entity = await DbContext.AiConversations
                .AsNoTracking()
                .Include( c => c.Messages )
                .FirstOrDefaultAsync( c => c.Id == id && c.UserId == user.Id )
                .DefaultConfigureAwait();

            if (entity is null)
            {
                return NotFound( $"ConversationIsNotFoundWithId {id}" );
            }

            return Ok( AiConversationDtoMapper.ToDto( entity ) );
        } );
    }

    [HttpGet( "by-client/{clientId}" )]
    public Task<IActionResult> GetByClientIdAsync( string clientId )
    {
        return TryCatchAsync( async user =>
        {
            string trimmed = ( clientId ?? string.Empty ).Trim();
            if (string.IsNullOrWhiteSpace( trimmed ))
            {
                return BadRequest( "ClientIdIsRequired" );
            }

            AiConversation? entity = await DbContext.AiConversations
                .AsNoTracking()
                .Include( c => c.Messages )
                .FirstOrDefaultAsync( c => c.UserId == user.Id && c.ClientId == trimmed )
                .DefaultConfigureAwait();

            if (entity is null)
            {
                return NotFound( $"ConversationIsNotFoundWithClientId {trimmed}" );
            }

            return Ok( AiConversationDtoMapper.ToDto( entity ) );
        } );
    }

    [HttpPost]
    public Task<IActionResult> CreateAsync( [FromBody] AiConversationDto request )
    {
        return TryCatchAsync( async user =>
        {
            if (request is null)
            {
                return BadRequest( "ConversationIsNull" );
            }

            string clientId = ( request.ClientId ?? string.Empty ).Trim();
            if (string.IsNullOrWhiteSpace( clientId ))
            {
                return BadRequest( "ClientIdIsRequired" );
            }

            if (clientId.Length > 64)
            {
                clientId = clientId[..64];
            }

            AiConversation? existing = await DbContext.AiConversations
                .Include( c => c.Messages )
                .FirstOrDefaultAsync( c => c.UserId == user.Id && c.ClientId == clientId )
                .DefaultConfigureAwait();

            if (existing is not null)
            {
                AiConversationDtoMapper.ApplyDto( existing, request );
                await DbContext.SaveChangesAsync().DefaultConfigureAwait();
                return Ok( AiConversationDtoMapper.ToDto( existing ) );
            }

            DateTime now = DateTime.UtcNow;
            AiConversation entity = new()
            {
                UserId = user.Id,
                ClientId = clientId,
                CreatedAt = request.CreatedAt != default
                    ? DateTime.SpecifyKind( request.CreatedAt, DateTimeKind.Utc )
                    : now,
                UpdatedAt = now,
            };
            AiConversationDtoMapper.ApplyDto( entity, request );
            if (entity.CreatedAt == default)
            {
                entity.CreatedAt = now;
            }

            await DbContext.AiConversations.AddAsync( entity ).DefaultConfigureAwait();
            await DbContext.SaveChangesAsync().DefaultConfigureAwait();

            return Created(
                $"api/ai/conversations/{entity.Id}",
                AiConversationDtoMapper.ToDto( entity ) );
        } );
    }

    [HttpPut( "{id:long}" )]
    public Task<IActionResult> UpdateAsync( long id, [FromBody] AiConversationDto request )
    {
        return TryCatchAsync( async user =>
        {
            if (id <= 0)
            {
                return BadRequest( "ConversationIdIsZeroOrNegative" );
            }

            if (request is null)
            {
                return BadRequest( "RequestIsNull" );
            }

            AiConversation? entity = await DbContext.AiConversations
                .Include( c => c.Messages )
                .FirstOrDefaultAsync( c => c.Id == id && c.UserId == user.Id )
                .DefaultConfigureAwait();

            if (entity is null)
            {
                return NotFound( $"ConversationIsNotFoundWithId {id}" );
            }

            AiConversationDtoMapper.ApplyDto( entity, request );
            await DbContext.SaveChangesAsync().DefaultConfigureAwait();

            return Ok( AiConversationDtoMapper.ToDto( entity ) );
        } );
    }

    [HttpPut( "by-client/{clientId}" )]
    public Task<IActionResult> UpsertByClientIdAsync( string clientId, [FromBody] AiConversationDto request )
    {
        return TryCatchAsync( async user =>
        {
            string trimmed = ( clientId ?? string.Empty ).Trim();
            if (string.IsNullOrWhiteSpace( trimmed ))
            {
                return BadRequest( "ClientIdIsRequired" );
            }

            if (request is null)
            {
                return BadRequest( "RequestIsNull" );
            }

            if (trimmed.Length > 64)
            {
                trimmed = trimmed[..64];
            }

            AiConversation? entity = await DbContext.AiConversations
                .Include( c => c.Messages )
                .FirstOrDefaultAsync( c => c.UserId == user.Id && c.ClientId == trimmed )
                .DefaultConfigureAwait();

            DateTime now = DateTime.UtcNow;
            if (entity is null)
            {
                entity = new AiConversation
                {
                    UserId = user.Id,
                    ClientId = trimmed,
                    CreatedAt = request.CreatedAt != default
                        ? DateTime.SpecifyKind( request.CreatedAt, DateTimeKind.Utc )
                        : now,
                    UpdatedAt = now,
                };
                AiConversationDtoMapper.ApplyDto( entity, request );
                await DbContext.AiConversations.AddAsync( entity ).DefaultConfigureAwait();
            }
            else
            {
                AiConversationDtoMapper.ApplyDto( entity, request );
            }

            await DbContext.SaveChangesAsync().DefaultConfigureAwait();
            return Ok( AiConversationDtoMapper.ToDto( entity ) );
        } );
    }

    [HttpDelete( "{id:long}" )]
    public Task<IActionResult> DeleteAsync( long id )
    {
        return TryCatchAsync( async user =>
        {
            if (id <= 0)
            {
                return BadRequest( "ConversationIdIsZeroOrNegative" );
            }

            AiConversation? entity = await DbContext.AiConversations
                .FirstOrDefaultAsync( c => c.Id == id && c.UserId == user.Id )
                .DefaultConfigureAwait();

            if (entity is null)
            {
                return NotFound( $"ConversationIsNotFoundWithId {id}" );
            }

            DbContext.SyncDeletions.Add( new SyncDeletion
            {
                UserId = user.Id,
                EntityType = SyncEntityTypes.Conversation,
                EntityId = entity.Id,
                DeletedAt = DateTime.UtcNow,
            } );
            DbContext.AiConversations.Remove( entity );
            await DbContext.SaveChangesAsync().DefaultConfigureAwait();
            return NoContent();
        } );
    }

    [HttpDelete( "by-client/{clientId}" )]
    public Task<IActionResult> DeleteByClientIdAsync( string clientId )
    {
        return TryCatchAsync( async user =>
        {
            string trimmed = ( clientId ?? string.Empty ).Trim();
            if (string.IsNullOrWhiteSpace( trimmed ))
            {
                return BadRequest( "ClientIdIsRequired" );
            }

            AiConversation? entity = await DbContext.AiConversations
                .FirstOrDefaultAsync( c => c.UserId == user.Id && c.ClientId == trimmed )
                .DefaultConfigureAwait();

            if (entity is null)
            {
                return NotFound( $"ConversationIsNotFoundWithClientId {trimmed}" );
            }

            DbContext.SyncDeletions.Add( new SyncDeletion
            {
                UserId = user.Id,
                EntityType = SyncEntityTypes.Conversation,
                EntityId = entity.Id,
                DeletedAt = DateTime.UtcNow,
            } );
            DbContext.AiConversations.Remove( entity );
            await DbContext.SaveChangesAsync().DefaultConfigureAwait();
            return NoContent();
        } );
    }
}
