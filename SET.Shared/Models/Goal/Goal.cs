using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SET.Shared.Models;

/// <summary>
/// Without long description, because  user should easily remember the target
/// </summary>
public class Goal
{
    public Guid Id { get; set; }
    [Required]
    public string ShortDescription { get; set; }
    [Required]
    public string ReasonToAchieve { get; set; }
    public DateTime CompletionTime { get; set; }
    public ActivityArea ActivityArea { get; set; }
    [Required]
    public GoalStatus Status { get; set; }

    public ICollection<Notice> Plan { get; set; }
    [Required]
    public Guid UserId { get; set; }
    public User User { get; set; }
}