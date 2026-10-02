using BusinessLogic.Models;

namespace BusinessLogic;

public static class AiConversationDtoMapper
{
    public static AiConversationDto ToDto( AiConversation entity, bool includeMessages = true )
    {
        return new AiConversationDto
        {
            Id = entity.Id,
            ClientId = entity.ClientId,
            Title = entity.Title ?? string.Empty,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            Messages = includeMessages
                ? ( entity.Messages ?? Array.Empty<AiMessage>() )
                    .OrderBy( m => m.SortOrder )
                    .ThenBy( m => m.CreatedAt )
                    .Select( ToMessageDto )
                    .ToList()
                : null,
        };
    }

    public static AiConversationMessageDto ToMessageDto( AiMessage entity )
    {
        return new AiConversationMessageDto
        {
            Id = entity.ClientId,
            Role = entity.Role,
            Content = entity.Content,
            SortOrder = entity.SortOrder,
            CreatedAt = entity.CreatedAt,
        };
    }

    public static void ApplyDto( AiConversation entity, AiConversationDto request )
    {
        string title = ( request.Title ?? string.Empty ).Trim();
        if (title.Length > 255)
        {
            title = title[..255];
        }

        entity.Title = title;
        if (request.UpdatedAt != default)
        {
            entity.UpdatedAt = DateTime.SpecifyKind( request.UpdatedAt, DateTimeKind.Utc );
        }
        else
        {
            entity.UpdatedAt = DateTime.UtcNow;
        }

        if (request.CreatedAt != default && entity.Id == 0)
        {
            entity.CreatedAt = DateTime.SpecifyKind( request.CreatedAt, DateTimeKind.Utc );
        }

        ApplyMessages( entity, request.Messages );
    }

    /// <summary>
    /// Null messages means an older client omitted the field — keep existing rows.
    /// A non-null list replaces all messages.
    /// </summary>
    public static void ApplyMessages( AiConversation entity, List<AiConversationMessageDto>? messages )
    {
        if (messages is null)
        {
            return;
        }

        entity.Messages ??= new List<AiMessage>();
        entity.Messages.Clear();

        var order = 0;
        foreach (AiConversationMessageDto item in messages)
        {
            string role = ( item.Role ?? string.Empty ).Trim().ToLowerInvariant();
            if (role is not ( "user" or "assistant" ))
            {
                continue;
            }

            string content = item.Content ?? string.Empty;
            string clientId = ( item.Id ?? string.Empty ).Trim();
            if (string.IsNullOrEmpty( clientId ))
            {
                clientId = Guid.NewGuid().ToString( "N" );
            }

            DateTime createdAt = item.CreatedAt != default
                ? DateTime.SpecifyKind( item.CreatedAt, DateTimeKind.Utc )
                : DateTime.UtcNow;

            entity.Messages.Add( new AiMessage
            {
                ClientId = clientId.Length > 64 ? clientId[..64] : clientId,
                Role = role,
                Content = content,
                SortOrder = item.SortOrder > 0 ? item.SortOrder : order,
                CreatedAt = createdAt,
            } );
            order++;
        }
    }
}
