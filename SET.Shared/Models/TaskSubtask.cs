using System.Collections.Generic;

namespace SET.Shared.Models;

public class TaskSubtask
{
    public long Id { get; set; }
    public long TaskId { get; set; }
    public Task Task { get; set; }

    /// Stable client-generated id (Flutter checklist row id).
    public string ClientId { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public int SortOrder { get; set; }
}
