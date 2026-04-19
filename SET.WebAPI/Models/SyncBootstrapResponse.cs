namespace SET.WebAPI.Models;

public class SyncBootstrapResponse
{
    public SyncUserDto User { get; set; }
    public List<SyncGoalDto> Goals { get; set; }
    public List<SyncHabitDto> ActiveHabits { get; set; }
    public List<SyncHabitDto> ArchivedHabits { get; set; }
    public SyncReminderDto? HabitsReportReminder { get; set; }
}

public class SyncUserDto
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string? MainSlogan { get; set; }
    public string? Mission { get; set; }
    public string Email { get; set; }
    public Gender Gender { get; set; }
    public DateTime LastModified { get; set; }
}

public class SyncGoalDto
{
    public long Id { get; set; }
    public string Name { get; set; }
    public DateTime LastModified { get; set; }
}

public class SyncHabitDto
{
    public long Id { get; set; }
    public string Name { get; set; }
    public TypeOfHabit Type { get; set; }
    public StatusOfHabit Status { get; set; }
    public List<SyncAreaOfLifeDto> AreasOfLife { get; set; }
    public int Priority { get; set; }
    public bool IsArchived { get; set; }
    public string? Description { get; set; }
    public SyncGoalDto? Goal { get; set; }
    public List<SyncHabitReminderDto> Reminders { get; set; }
    public DateTime? ArchivingTime { get; set; }
    public List<SyncProgressDto> Progresses { get; set; }
    public SyncFrequencyDto Frequency { get; set; }
    public string? ColorName { get; set; }
    public int Complexity { get; set; }
    public DateTime LastModified { get; set; }
}

public class SyncFrequencyDto
{
    public long Id { get; set; }
    public FrequencyType Type { get; set; }
    public int Repeats { get; set; }
    public int IntervalLengthInDays { get; set; }
    public DateTime LastModified { get; set; }
}

public class SyncAreaOfLifeDto
{
    public long Id { get; set; }
    public string Name { get; set; }
    public DateTime LastModified { get; set; }
}

public class SyncHabitReminderDto
{
    public long Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public TimeOnly Time { get; set; }
    public bool IsEnabled { get; set; }
    public List<SyncWeekDayDto> DaysOfWeek { get; set; }
    public DateTime LastModified { get; set; }
}

public class SyncWeekDayDto
{
    public long Id { get; set; }
    public DayOfWeek Type { get; set; }
    public int UserNotificationRequestId { get; set; }
    public DateTime LastModified { get; set; }
}

public class SyncProgressDto
{
    public long Id { get; set; }
    public DateOnly Date { get; set; }
    public int Value { get; set; }
    public DateTime LastModified { get; set; }
}

public class SyncReminderDto
{
    public long Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public TimeOnly Time { get; set; }
    public bool IsEnabled { get; set; }
    public int UserNotificationRequestId { get; set; }
    public DateTime LastModified { get; set; }
}
