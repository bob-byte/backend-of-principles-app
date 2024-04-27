using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET.Shared.Models;

public class UserHabitInProgressShortDto : EntityWithId
{
    public string Name { get; set; }
    public double PercentageAchieved { get; set; }
    public ICollection<ProgressOfHabit> Progresses { get; set; }
    public Frequency Frequency { get; set; }
    public ICollection<UserAreaOfLife> AreasOfLife { get; set; }
    public int Complexity { get; set; }
    public string ColorName { get; set; }
    public StatusOfHabit Status { get; set; }
    public int Priority { get; set; }
}
