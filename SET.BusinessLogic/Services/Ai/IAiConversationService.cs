namespace BusinessLogic;

/// <summary>AI Helper chat history; <c>clientId</c> is the Flutter conversation UUID.</summary>
public interface IAiConversationService
{
    Task<List<AiConversationDto>> GetAllAsync( long userId );

    Task<ServiceResult<AiConversationDto>> GetByIdAsync( long userId, long id );

    Task<ServiceResult<AiConversationDto>> GetByClientIdAsync( long userId, string clientId );

    Task<ServiceResult<AiConversationCreateResult>> CreateAsync( long userId, AiConversationDto request );

    Task<ServiceResult<AiConversationDto>> UpdateAsync( long userId, long id, AiConversationDto request );

    Task<ServiceResult<AiConversationDto>> UpsertByClientIdAsync( long userId, string clientId, AiConversationDto request );

    Task<ServiceResult> DeleteAsync( long userId, long id );

    Task<ServiceResult> DeleteByClientIdAsync( long userId, string clientId );
}

/// <param name="IsCreated">False when a conversation with the same client id already existed and was updated.</param>
public sealed record AiConversationCreateResult( AiConversationDto Conversation, bool IsCreated );
