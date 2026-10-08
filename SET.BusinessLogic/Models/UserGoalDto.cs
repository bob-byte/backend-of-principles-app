namespace BusinessLogic.Models;

public class UserGoalDto
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string? Notes { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsArchived { get; set; }
    public DateOnly? Deadline { get; set; }
    public TimeOnly? DeadlineTime { get; set; }
    public List<TaskReminderOffsetDto> Reminders { get; set; } = new();

    /// <summary>
    /// Checklist steps. Null means an older client omitted the field — keep stored rows.
    /// An empty list clears them.
    /// </summary>
    public List<GoalSubgoalDto>? Subgoals { get; set; }

    public DateTime LastModified { get; set; }
}

public class GoalSubgoalDto
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public bool IsCompleted { get; set; }
    public int SortOrder { get; set; }
}
