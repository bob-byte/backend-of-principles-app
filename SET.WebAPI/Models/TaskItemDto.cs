namespace SET.WebAPI.Models;

public class TaskItemDto
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string? Notes { get; set; }
    public DateOnly? Date { get; set; }
    public TimeOnly? Time { get; set; }
    public bool IsCompleted { get; set; }
}

