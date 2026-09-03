using System.Collections.Generic;

namespace SET.WebAPI.Models;

public class AiChatRequest
{
    public List<AiChatMessageDto> Messages { get; set; }
    public string Prompt { get; set; }
}

public class AiChatMessageDto
{
    public string Role { get; set; }
    public string Content { get; set; }
}

public class AiChatResponse
{
    public string Content { get; set; }
}

public class AiParseTaskRequest
{
    public string Prompt { get; set; }
}

public class AiTaskDraftDto
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string Priority { get; set; }
    public string Theme { get; set; }
    public string DueDate { get; set; }
}

public class AiRecommendHabitsRequest
{
    public string Culture { get; set; }
    public string Goal { get; set; }
    public List<string> Goals { get; set; }
    public List<string> CurrentHabits { get; set; }
    public List<string> AreasOfLife { get; set; }
    public string Mission { get; set; }
    public string MainSlogan { get; set; }
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
