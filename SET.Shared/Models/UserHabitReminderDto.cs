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
    public IEnumerable<DayOfWeek> DaysOfWeek { get; set; }
}
