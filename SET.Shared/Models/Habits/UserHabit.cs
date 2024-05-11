using SET.Shared.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET.Shared.Models;

public class UserHabit : EntityWithId
{
    public string Name { get; set; }
    public TypeOfHabit Type { get; set; }
    public StatusOfHabit Status { get; set; }
    public long UserId { get; set; }
    public User User { get; set; }
    public ICollection<UserAreaOfLifeUserHabit> AreasOfLife { get; set; }
    public string? Description { get; set; }
    public string ReasonToFollow { get; set; }
    public string? Question { get; set; }
    public double PercentageAchieved { get; set; }
    public ICollection<ProgressOfHabit> Progresses { get; set; }
    public ICollection<UserHabit>? ParentHabits { get; set; }
    public ICollection<UserHabit>? SubHabits { get; set; }
    public Frequency Frequency { get; set; }
    public long FrequencyId { get; set; }
    public int Priority { get; set; }
    public int Complexity { get; set; }
    public string ColorName { get; set; }
}
