namespace SET.Shared.Models;

public class GoalSubgoal
{
    public long Id { get; set; }
    public long GoalId { get; set; }
    public UserGoal Goal { get; set; }

    /// <summary>Stable client-generated id (Flutter checklist row id).</summary>
    public string ClientId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public int SortOrder { get; set; }
}
