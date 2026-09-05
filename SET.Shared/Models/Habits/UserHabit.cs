using SET.Shared.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET.Shared.Models;

public class UserHabit
{
    public long Id { get; set; }
    public string Name { get; set; }
    public TypeOfHabit Type { get; set; }
    public StatusOfHabit Status { get; set; }
    public string? ReasonToFollow { get; set; }
    public long UserId { get; set; }
    public User User { get; set; }
    public ICollection<UserAreaOfLifeUserHabit> AreasOfLife { get; set; }
    public string? Description { get; set; }
    public string? Question { get; set; }
    public ICollection<ProgressOfHabit> Progresses { get; set; }
    public Frequency Frequency { get; set; }
    public long FrequencyId { get; set; }
    public int Priority { get; set; }
    public int Complexity { get; set; }
    public string ColorName { get; set; }
    public long? GoalId { get; set; }
    public UserGoal? Goal { get; set; }
    public bool IsArchived { get; set; }
    public DateTime? ArchivingTime { get; set; }
    public ICollection<UserHabitReminder> Reminders { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateOnly? EndDate { get; set; }
    public TimeOnly? EndTime { get; set; }
    public bool AllDay { get; set; }
    public bool ConstantReminder { get; set; }
    public HabitKind Kind { get; set; }
    public string? Unit { get; set; }
    public double? TargetPerOneTime { get; set; }
    public NumericalHabitType TargetType { get; set; }
    public double? MinRate { get; set; }
    public double? MaxRate { get; set; }
    public ProgressMarkVariaty ProgressMarkVariaty { get; set; }
}
