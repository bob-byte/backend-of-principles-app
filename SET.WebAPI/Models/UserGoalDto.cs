namespace SET.WebAPI.Models;

public class UserGoalDto
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string? Notes { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsArchived { get; set; }
    public DateTime LastModified { get; set; }
}
