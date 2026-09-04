using System;
using System.Collections.Generic;

namespace SET.Shared.Models;
public class UserHabitReminder
{
    public long Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public TimeOnly Time { get; set; }
    public bool IsEnabled { get; set; }
    public long UserHabitId { get; set; }
    public UserHabit UserHabit { get; set; }
    public ICollection<WeekDay> DaysOfWeek { get; set; }

    /// JSON array of { offsetMinutes, notificationRequestId }.
    public string? OffsetsJson { get; set; }
    public bool ConstantReminder { get; set; }
    public int? ConstantNotificationRequestId { get; set; }
    public TimeOnly? EndTime { get; set; }
    public bool AllDay { get; set; }
}
