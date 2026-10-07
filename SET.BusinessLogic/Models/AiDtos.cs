using System;
using System.Collections.Generic;

namespace BusinessLogic.Models;

public class AiChatRequest
{
    public List<AiChatMessageDto>? Messages { get; set; }

    /// <summary>Optional; Flutter sends the latest turn inside <see cref="Messages"/> only.</summary>
    public string? Prompt { get; set; }
}

public class AiChatMessageDto
{
    public string Role { get; set; }
    public string Content { get; set; }
    public List<AiChatAttachmentDto>? Attachments { get; set; }
}

public class AiChatAttachmentDto
{
    public string? FileName { get; set; }
    public string? MimeType { get; set; }
    public string? Data { get; set; }
}

public class AiChatResponse
{
    public string Content { get; set; }
}

public class AiParseTaskRequest
{
    public string Prompt { get; set; }

    /// Client local calendar date (<c>yyyy-MM-dd</c>) for relative dates.
    public string? LocalDate { get; set; }

    /// Minutes east of UTC (e.g. Kyiv winter = 120).
    public int? UtcOffsetMinutes { get; set; }
}

public class AiTaskDraftDto
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string Priority { get; set; }
    public string Theme { get; set; }

    /// Local wall-clock due: <c>yyyy-MM-dd</c> or <c>yyyy-MM-ddTHH:mm</c>.
    public string DueDate { get; set; }

    public bool? AllDay { get; set; }
    public List<int> Reminders { get; set; }
    public List<string> Subtasks { get; set; }
}

public class AiRecommendHabitsRequest
{
    public string Culture { get; set; }
    public string? Goal { get; set; }
    public List<string>? Goals { get; set; }
    public List<string>? CurrentHabits { get; set; }
    public List<string>? AreasOfLife { get; set; }
    public string? Mission { get; set; }
    public string? MainSlogan { get; set; }
    public int? Gender { get; set; }
}

public class AiRecommendHabitsResponse
{
    public List<AiRecommendedHabitDto> Habits { get; set; }
}

public class AiRecommendedHabitDto
{
    public string Name { get; set; }
    public string ReasonToFollow { get; set; }
}

public class AiTitleRequest
{
    public string UserMessage { get; set; }
    public string? AssistantMessage { get; set; }
}

public class AiTitleResponse
{
    public string Title { get; set; }
}

public class AiSuggestProfileTextRequest
{
    /// <summary>"slogan" or "mission".</summary>
    public string Kind { get; set; }
    public string Culture { get; set; }
    public string? Hint { get; set; }
    public string? Draft { get; set; }
    public string? Mission { get; set; }
    public string? MainSlogan { get; set; }
    public List<string>? Goals { get; set; }
    public int? Gender { get; set; }
}

public class AiSuggestProfileTextResponse
{
    public List<AiProfileTextSuggestionDto> Suggestions { get; set; }
}

public class AiProfileTextSuggestionDto
{
    public string Text { get; set; }
    public string Reason { get; set; }
}

public class AiRecommendGoalsRequest
{
    public string Culture { get; set; }
    public string AreaOfLife { get; set; }
    public List<string>? ExistingGoals { get; set; }
    public string? Draft { get; set; }
    public string? Mission { get; set; }
    public string? MainSlogan { get; set; }
    public int? Gender { get; set; }
}

public class AiRecommendGoalsResponse
{
    public List<AiRecommendedGoalDto> Goals { get; set; }
}

public class AiRecommendedGoalDto
{
    public string Name { get; set; }
    public string Reason { get; set; }
}

public class AiConversationDto
{
    public long Id { get; set; }
    public string ClientId { get; set; }
    public string Title { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<AiConversationMessageDto> Messages { get; set; }
}

public class AiConversationMessageDto
{
    public string Id { get; set; }
    public string Role { get; set; }
    public string Content { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
}
