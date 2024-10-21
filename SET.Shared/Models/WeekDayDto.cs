using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET.Shared.Models;
public class WeekDayDto
{
    public long Id { get; set; }
    public DayOfWeek Type { get; set; }
    public long UserNotificationRequestId { get; set; }
}
