using System.Collections.Generic;
using System.Threading;

namespace BusinessLogic;

public interface IAiService
{
    Task<string> CompleteChatAsync(
        IReadOnlyList<AiChatMessage> messages,
        CancellationToken cancellationToken = default );

    IAsyncEnumerable<string> StreamChatAsync(
        IReadOnlyList<AiChatMessage> messages,
        CancellationToken cancellationToken = default );

    Task<AiTaskDraftResult> ParseTaskDraftAsync(
        string prompt,
        CancellationToken cancellationToken = default );

    Task<IReadOnlyList<RecommendedHabitResult>> RecommendHabitsAsync(
        RecommendHabitsContext context,
        CancellationToken cancellationToken = default );
}

public sealed class AiChatMessage
{
    public AiChatMessage( string role, string content )
    {
        Role = role;
        Content = content;
    }

    public string Role { get; }
    public string Content { get; }
}

public sealed class AiTaskDraftResult
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? Priority { get; init; }
    public string? Theme { get; init; }
    public string? DueDate { get; init; }
}

public sealed class RecommendHabitsContext
{
    public string Culture { get; init; } = "uk";
    public string? Goal { get; init; }
    public IReadOnlyList<string> Goals { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> CurrentHabits { get; init; } = Array.Empty<string>();
    public string? Mission { get; init; }
    public string? MainSlogan { get; init; }
    public string Gender { get; init; } = "other";
}

public sealed class RecommendedHabitResult
{
    public string Name { get; init; } = string.Empty;
    public string ReasonToFollow { get; init; } = string.Empty;
}

public sealed class AiServiceException : Exception
{
    public AiServiceException( string message, int statusCode = 502 )
        : base( message )
    {
        StatusCode = statusCode;
    }

    public int StatusCode { get; }
}
