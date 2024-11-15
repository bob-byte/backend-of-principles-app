using System;

namespace SET.Shared.Models;
public class UserReminderDto
{
    public long Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public TimeOnly Time { get; set; }
    public bool IsEnabled { get; set; }
    public int UserNotificationRequestId { get; set; }
}
