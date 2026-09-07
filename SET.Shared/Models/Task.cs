using System;
using System.Collections.Generic;

namespace SET.Shared.Models;

public class Task
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public User User { get; set; }
    public string Name { get; set; }
    public string? Notes { get; set; }
    public DateOnly? Date { get; set; }
    public TimeOnly? Time { get; set; }
    public DateOnly? EndDate { get; set; }
    public TimeOnly? EndTime { get; set; }
    public bool AllDay { get; set; }
    public bool IsCompleted { get; set; } = false;
    public bool ConstantReminder { get; set; }
    public int? ConstantNotificationRequestId { get; set; }

    /// JSON array of { offsetMinutes, notificationRequestId }.
    public string? RemindersJson { get; set; }

    /// JSON object for repeat config.
    public string? RepeatJson { get; set; }

    public ICollection<TaskSubtask> Subtasks { get; set; } = new List<TaskSubtask>();
}
