using System;
using System.Collections.Generic;

namespace SET.Shared.Models;

public class EditUserHabitDto : EntityWithId
{
    public string Name { get; set; }
    public TypeOfHabit Type { get; set; }
    public ICollection<UserAreaOfLife> AreasOfLife { get; set; }
    public string? Description { get; set; }
    public string ReasonToFollow { get; set; }
    public string? Question { get; set; }
    public Frequency Frequency { get; set; }
    public int Priority { get; set; }
    public string ColorName { get; set; }

    public List<UserHabitWithPriority> PrioritizedHabits { get; set; }
}
