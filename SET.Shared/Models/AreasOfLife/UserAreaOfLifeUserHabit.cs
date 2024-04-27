using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET.Shared.Models;

public class UserAreaOfLifeUserHabit
{
    public Guid Id { get; set; }
    public Guid AreaOfLifeId { get; set; }
    public virtual UserAreaOfLife AreaOfLife { get; set; }
    public Guid HabitId { get; set; }
    public virtual UserHabit Habit { get; set; }
    public int PriorityOfHabit { get; set; }
}
