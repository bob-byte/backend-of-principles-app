namespace BusinessLogic;

public class AiConversationService : IAiConversationService
{
    private const int MaxClientIdLength = 64;

    private readonly AppDbContext m_dbContext;

    public AiConversationService( AppDbContext dbContext )
    {
        m_dbContext = dbContext;
    }

    public async Task<List<AiConversationDto>> GetAllAsync( long userId )
    {
        List<AiConversation> entities = await m_dbContext.AiConversations
            .AsNoTracking()
            .Include( c => c.Messages )
            .Where( c => c.UserId == userId )
            .OrderByDescending( c => c.UpdatedAt )
            .ToListAsync()
            .DefaultConfigureAwait();

        return entities.Select( c => AiConversationDtoMapper.ToDto( c ) ).ToList();
    }

    public async Task<ServiceResult<AiConversationDto>> GetByIdAsync( long userId, long id )
    {
        if (id <= 0)
        {
            return ServiceError.BadRequest( "ConversationIdIsZeroOrNegative" );
        }

        AiConversation? entity = await m_dbContext.AiConversations
            .AsNoTracking()
            .Include( c => c.Messages )
            .FirstOrDefaultAsync( c => c.Id == id && c.UserId == userId )
            .DefaultConfigureAwait();

        if (entity is null)
        {
            return ServiceError.NotFound( $"ConversationIsNotFoundWithId {id}" );
        }

        return AiConversationDtoMapper.ToDto( entity );
    }

    public async Task<ServiceResult<AiConversationDto>> GetByClientIdAsync( long userId, string clientId )
    {
        string trimmed = ( clientId ?? string.Empty ).Trim();
        if (string.IsNullOrWhiteSpace( trimmed ))
        {
            return ServiceError.BadRequest( "ClientIdIsRequired" );
        }

        AiConversation? entity = await m_dbContext.AiConversations
            .AsNoTracking()
            .Include( c => c.Messages )
            .FirstOrDefaultAsync( c => c.UserId == userId && c.ClientId == trimmed )
            .DefaultConfigureAwait();

        if (entity is null)
        {
            return ServiceError.NotFound( $"ConversationIsNotFoundWithClientId {trimmed}" );
        }

        return AiConversationDtoMapper.ToDto( entity );
    }

    public async Task<ServiceResult<AiConversationCreateResult>> CreateAsync( long userId, AiConversationDto request )
    {
        if (request is null)
        {
            return ServiceError.BadRequest( "ConversationIsNull" );
        }

        string clientId = ( request.ClientId ?? string.Empty ).Trim();
        if (string.IsNullOrWhiteSpace( clientId ))
        {
            return ServiceError.BadRequest( "ClientIdIsRequired" );
        }

        if (clientId.Length > MaxClientIdLength)
        {
            clientId = clientId[..MaxClientIdLength];
        }

        AiConversation? existing = await FindTrackedByClientIdAsync( userId, clientId ).DefaultConfigureAwait();
        if (existing is not null)
        {
            AiConversationDtoMapper.ApplyDto( existing, request );
            await m_dbContext.SaveChangesAsync().DefaultConfigureAwait();
            return new AiConversationCreateResult( AiConversationDtoMapper.ToDto( existing ), IsCreated: false );
        }

        AiConversation entity = NewConversation( userId, clientId, request );
        if (entity.CreatedAt == default)
        {
            entity.CreatedAt = DateTime.UtcNow;
        }

        await m_dbContext.AiConversations.AddAsync( entity ).DefaultConfigureAwait();
        await m_dbContext.SaveChangesAsync().DefaultConfigureAwait();

        return new AiConversationCreateResult( AiConversationDtoMapper.ToDto( entity ), IsCreated: true );
    }

    public async Task<ServiceResult<AiConversationDto>> UpdateAsync( long userId, long id, AiConversationDto request )
    {
        if (id <= 0)
        {
            return ServiceError.BadRequest( "ConversationIdIsZeroOrNegative" );
        }

        if (request is null)
        {
            return ServiceError.BadRequest( "RequestIsNull" );
        }

        AiConversation? entity = await m_dbContext.AiConversations
            .Include( c => c.Messages )
            .FirstOrDefaultAsync( c => c.Id == id && c.UserId == userId )
            .DefaultConfigureAwait();

        if (entity is null)
        {
            return ServiceError.NotFound( $"ConversationIsNotFoundWithId {id}" );
        }

        AiConversationDtoMapper.ApplyDto( entity, request );
        await m_dbContext.SaveChangesAsync().DefaultConfigureAwait();

        return AiConversationDtoMapper.ToDto( entity );
    }

    public async Task<ServiceResult<AiConversationDto>> UpsertByClientIdAsync( long userId, string clientId, AiConversationDto request )
    {
        string trimmed = ( clientId ?? string.Empty ).Trim();
        if (string.IsNullOrWhiteSpace( trimmed ))
        {
            return ServiceError.BadRequest( "ClientIdIsRequired" );
        }

        if (request is null)
        {
            return ServiceError.BadRequest( "RequestIsNull" );
        }

        if (trimmed.Length > MaxClientIdLength)
        {
            trimmed = trimmed[..MaxClientIdLength];
        }

        AiConversation? entity = await FindTrackedByClientIdAsync( userId, trimmed ).DefaultConfigureAwait();
        if (entity is null)
        {
            entity = NewConversation( userId, trimmed, request );
            await m_dbContext.AiConversations.AddAsync( entity ).DefaultConfigureAwait();
        }
        else
        {
            AiConversationDtoMapper.ApplyDto( entity, request );
        }

        await m_dbContext.SaveChangesAsync().DefaultConfigureAwait();
        return AiConversationDtoMapper.ToDto( entity );
    }

    public async Task<ServiceResult> DeleteAsync( long userId, long id )
    {
        if (id <= 0)
        {
            return ServiceError.BadRequest( "ConversationIdIsZeroOrNegative" );
        }

        AiConversation? entity = await m_dbContext.AiConversations
            .FirstOrDefaultAsync( c => c.Id == id && c.UserId == userId )
            .DefaultConfigureAwait();

        if (entity is null)
        {
            return ServiceError.NotFound( $"ConversationIsNotFoundWithId {id}" );
        }

        await RemoveWithTombstoneAsync( userId, entity ).DefaultConfigureAwait();
        return ServiceResult.Success;
    }

    public async Task<ServiceResult> DeleteByClientIdAsync( long userId, string clientId )
    {
        string trimmed = ( clientId ?? string.Empty ).Trim();
        if (string.IsNullOrWhiteSpace( trimmed ))
        {
            return ServiceError.BadRequest( "ClientIdIsRequired" );
        }

        AiConversation? entity = await m_dbContext.AiConversations
            .FirstOrDefaultAsync( c => c.UserId == userId && c.ClientId == trimmed )
            .DefaultConfigureAwait();

        if (entity is null)
        {
            return ServiceError.NotFound( $"ConversationIsNotFoundWithClientId {trimmed}" );
        }

        await RemoveWithTombstoneAsync( userId, entity ).DefaultConfigureAwait();
        return ServiceResult.Success;
    }

    private Task<AiConversation?> FindTrackedByClientIdAsync( long userId, string clientId )
    {
        return m_dbContext.AiConversations
            .Include( c => c.Messages )
            .FirstOrDefaultAsync( c => c.UserId == userId && c.ClientId == clientId );
    }

    private static AiConversation NewConversation( long userId, string clientId, AiConversationDto request )
    {
        DateTime now = DateTime.UtcNow;
        AiConversation entity = new()
        {
            UserId = userId,
            ClientId = clientId,
            CreatedAt = request.CreatedAt != default
                ? DateTime.SpecifyKind( request.CreatedAt, DateTimeKind.Utc )
                : now,
            UpdatedAt = now,
        };
        AiConversationDtoMapper.ApplyDto( entity, request );
        return entity;
    }

    private async Task RemoveWithTombstoneAsync( long userId, AiConversation entity )
    {
        m_dbContext.SyncDeletions.Add( new SyncDeletion
        {
            UserId = userId,
            EntityType = SyncEntityTypes.Conversation,
            EntityId = entity.Id,
            DeletedAt = DateTime.UtcNow,
        } );
        m_dbContext.AiConversations.Remove( entity );
        await m_dbContext.SaveChangesAsync().DefaultConfigureAwait();
    }
}
