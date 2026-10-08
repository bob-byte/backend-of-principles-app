using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET.Shared.Models;
public class UserGoal
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string? Notes { get; set; }
    public long UserId { get; set; }
    public User User { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsArchived { get; set; }
    public DateTime? ArchivingTime { get; set; }

    /// <summary>Optional goal deadline (calendar day).</summary>
    public DateOnly? Deadline { get; set; }

    /// <summary>Optional local clock time. Null means reminders fire at 09:00.</summary>
    public TimeOnly? DeadlineTime { get; set; }

    /// <summary>JSON array of { offsetMinutes, notificationRequestId } before the deadline day.</summary>
    public string? RemindersJson { get; set; }

    public ICollection<UserHabit>? UserHabits { get; set; }
}
