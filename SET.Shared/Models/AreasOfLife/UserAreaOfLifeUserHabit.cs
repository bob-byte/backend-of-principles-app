using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET.Shared.Models;

public class UserAreaOfLifeUserHabit
{
    public long Id { get; set; }
    public long AreaOfLifeId { get; set; }
    public virtual UserAreaOfLife AreaOfLife { get; set; }
    public long HabitId { get; set; }
    public virtual UserHabit Habit { get; set; }
}
