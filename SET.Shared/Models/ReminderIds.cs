using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET.Shared.Models;
public class ReminderIds
{
    public long Id { get; set; }
    public List<WeekDayIds> DaysOfWeek { get; set; }
}

public class WeekDayIds
{
    public long Id { get; set; }
    public int NotificationRequestId { get; set; }
}
