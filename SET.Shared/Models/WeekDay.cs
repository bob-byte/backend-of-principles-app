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
    public ICollection<UserHabitReminder> Reminders { get; set; }
}
