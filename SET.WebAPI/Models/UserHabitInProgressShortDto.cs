
namespace SET.WebAPI.Models;

public class UserHabitInProgressShortDto
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
    public IEnumerable<UserAreaOfLifeDto> AreasOfLife { get; set; }
    public IEnumerable<ProgressOfHabitDto> Progresses { get; set; }
    public FrequencyDto Frequency { get; set; }
    public int Complexity { get; set; }
    public int Priority { get; set; }
    public UserGoalDto? Goal { get; set; }
    public IEnumerable<UserHabitReminderDto> Reminders { get; set; }
    public string? Description { get; set; }
    public string ColorName { get; set; }
    public DateTime LastModified { get; set; }
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
