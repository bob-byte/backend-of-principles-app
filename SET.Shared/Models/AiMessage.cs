using System;

namespace SET.Shared.Models;

public class AiMessage
{
    public long Id { get; set; }

    public long ConversationId { get; set; }
    public AiConversation Conversation { get; set; }

    /// <summary>Stable client-generated id (Flutter message UUID).</summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>user or assistant.</summary>
    public string Role { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; }
}
