namespace SET.WebAPI.Models;

public class SyncBootstrapUserDto
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string MainSlogan { get; set; }
    public string Mission { get; set; }
    public string Email { get; set; }
    public Gender Gender { get; set; }
    public DateTime LastModified { get; set; }
}

public class SyncBootstrapArchivedHabitDto
{
    public long Id { get; set; }
    public string Name { get; set; }
    public bool IsArchived { get; set; } = true;
    public DateTime LastModified { get; set; }
}

public class SyncBootstrapResponse
{
    public SyncBootstrapUserDto User { get; set; }
    public List<UserGoalDto> Goals { get; set; }
    public List<UserHabitInProgressShortDto> ActiveHabits { get; set; }
    public List<SyncBootstrapArchivedHabitDto> ArchivedHabits { get; set; }
    public UserReminderDto HabitsReportReminder { get; set; }

    /// Same payload as GET api/reminder/all — included so clients can restore
    /// device notifications after bootstrap without a second round-trip.
    public List<UserReminderDto> GeneralReminders { get; set; } = new();
    public List<UserHabitReminderDto> UserHabitReminders { get; set; } = new();

    public List<TaskItemDto> Tasks { get; set; }
    public List<AiConversationDto> Conversations { get; set; }
}
