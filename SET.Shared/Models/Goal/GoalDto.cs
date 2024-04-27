using System;

namespace SET.Shared.Models;

public class GoalDto
{
    public string ShortDescription { get; set; }
    public string ReasonToAchieve { get; set; }
    public DateTime CompletionTime { get; set; }
    public ActivityArea ActivityArea { get; set; }
    public GoalStatus Status { get; set; }
}
