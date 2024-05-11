using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET.Shared.Models;

public class ProgressOfHabit : EntityWithId
{
    public const int UNKNOWN = -1;
    public const int NO = 0;
    public const int YES_AUTO = 1;
    public const int YES_MANUAL = 2;
    public const int SKIP = 3;

    public bool IsCompleted { get; set; }
    public DateOnly Date { get; set; }
    public int Value { get; set; }
    public UserHabit Habit { get; set; }
    public long HabitId { get; set; }
}
