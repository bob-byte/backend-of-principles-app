using System;

namespace SET.WebAPI.Models;

public class TaskReminderOffsetDto
{
    public int OffsetMinutes { get; set; }
    public int? NotificationRequestId { get; set; }
}

public class TaskRepeatDto
{
    public string Preset { get; set; } = "none";
    public int Interval { get; set; } = 1;
    public string Unit { get; set; } = "day";
    public int[] Weekdays { get; set; } = Array.Empty<int>();
    public string Anchor { get; set; } = "dueDates";
}

public class TaskItemDto
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string? Notes { get; set; }
    public DateOnly? Date { get; set; }
    public TimeOnly? Time { get; set; }
    public DateOnly? EndDate { get; set; }
    public TimeOnly? EndTime { get; set; }
    public bool AllDay { get; set; }
    public bool IsCompleted { get; set; }
    public bool ConstantReminder { get; set; }
    public int? ConstantNotificationRequestId { get; set; }
    public List<TaskReminderOffsetDto> Reminders { get; set; } = new();
    public TaskRepeatDto? Repeat { get; set; }
}
