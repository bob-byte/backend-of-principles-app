using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET.Shared.Models;
public class WeekDay
{
    public long Id { get; set; }
    public DayOfWeek Type { get; set; }
    public long UserHabitReminderId { get; set; }
    public UserHabitReminder UserHabitReminder { get; set; }
    public int UserNotificationRequestId { get; set; }
}
