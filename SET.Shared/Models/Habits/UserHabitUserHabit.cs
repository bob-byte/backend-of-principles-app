using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET.Shared.Models;
public class UserHabitUserHabit
{
    public Guid ParentHabitId { get; set; }
    public UserHabit ParentHabit { get; set; }

    public Guid SubHabitId { get; set; }
    public UserHabit SubHabit { get; set; }
}
