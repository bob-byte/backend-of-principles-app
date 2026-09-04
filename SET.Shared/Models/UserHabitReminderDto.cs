using System;
using System.Collections.Generic;

namespace SET.Shared.Models;
public class UserHabitReminderDto
{
    public long Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public TimeOnly Time { get; set; }
    public bool IsEnabled { get; set; }
    public WeekDayDto[] DaysOfWeek { get; set; }
    public List<ReminderOffsetDto>? Offsets { get; set; }
    public bool ConstantReminder { get; set; }
    public int? ConstantNotificationRequestId { get; set; }
    public TimeOnly? EndTime { get; set; }
    public bool AllDay { get; set; }
}

public class ReminderOffsetDto
{
    public int OffsetMinutes { get; set; }
    public int? NotificationRequestId { get; set; }
}
