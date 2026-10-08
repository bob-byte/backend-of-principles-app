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
    public DateTime LastModified { get; set; }
}
