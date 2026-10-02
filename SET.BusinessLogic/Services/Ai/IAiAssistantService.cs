using System.Threading;

namespace BusinessLogic;

/// <summary>
/// AI endpoints on top of <see cref="IAiService"/>: sanitizes requests, fills missing context
/// (habits, goals, tasks, profile) from the user's data, and maps results to API DTOs.
/// AI failures come back as <see cref="ServiceError"/> with an <c>{ error }</c> body.
/// </summary>
public interface IAiAssistantService
{
    Task<ServiceResult<AiChatSession>> PrepareChatAsync( User user, AiChatRequest request );

    IAsyncEnumerable<string> StreamChatAsync( AiChatSession session, CancellationToken cancellationToken );

    Task<ServiceResult<AiTaskDraftDto>> ParseTaskAsync( AiParseTaskRequest request, CancellationToken cancellationToken );

    Task<ServiceResult<AiRecommendHabitsResponse>> RecommendHabitsAsync(
        User user,
        AiRecommendHabitsRequest request,
        CancellationToken cancellationToken );

    Task<ServiceResult<AiTitleResponse>> GenerateTitleAsync( AiTitleRequest request, CancellationToken cancellationToken );

    Task<ServiceResult<AiSuggestProfileTextResponse>> SuggestProfileTextAsync(
        User user,
        AiSuggestProfileTextRequest request,
        CancellationToken cancellationToken );

    Task<ServiceResult<AiRecommendGoalsResponse>> RecommendGoalsAsync(
        User user,
        AiRecommendGoalsRequest request,
        CancellationToken cancellationToken );
}

public sealed record AiChatSession( IReadOnlyList<AiChatMessage> Messages, ChatUserContext UserContext );
