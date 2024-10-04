using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET.Shared.Models;
public class UserHabitReminder
{
    public long Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime Time { get; set; }
    public bool IsEnabled { get; set; }
    public long UserHabitId { get; set; }
    public UserHabit UserHabit { get; set; }
    public ICollection<WeekDay> DaysOfWeek { get; set; }
}
