
namespace SET.WebAPI.Models;

public class EditUserHabitDto
{
    public class FrequencyDto
    {
        public long Id { get; set; }
        public FrequencyType Type { get; set; }
        public int Repeats { get; set; }
        public int IntervalLengthInDays { get; set; }
    }

    public long Id { get; set; }
    public string Name { get; set; }
    public TypeOfHabit Type { get; set; }
    public string? ReasonToFollow { get; set; }
    public ICollection<UserAreaOfLifeDto> AreasOfLife { get; set; }
    public string? Description { get; set; }
    public int Complexity { get; set; }
    public string? Question { get; set; }
    public StatusOfHabit Status { get; set; }
    public FrequencyDto Frequency { get; set; }
    public int Priority { get; set; }
    public string ColorName { get; set; }
    public UserGoalDto? Goal { get; set; }
    public List<UserHabitWithPriority> PrioritizedHabits { get; set; }
}
