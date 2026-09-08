using System;
using System.Collections.Generic;

namespace SET.Shared.Models;

public class AiConversation
{
    public long Id { get; set; }

    /// <summary>Stable client-generated id (Flutter conversation UUID).</summary>
    public string ClientId { get; set; } = string.Empty;

    public long UserId { get; set; }
    public User User { get; set; }

    public string Title { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<AiMessage> Messages { get; set; } = new List<AiMessage>();
}
