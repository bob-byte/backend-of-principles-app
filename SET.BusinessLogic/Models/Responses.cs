namespace BusinessLogic.Models;

public record LoginResponse(string Token);

public class ProgressSavedResponse
{
    public long Id { get; set; }
}

public class HabitSavedResponse
{
    public long Id { get; set; }
    public long FrequencyId { get; set; }
    public List<ReminderIds> ReminderIds { get; set; }
}

public class ArchivedHabitResponse
{
    public long Id { get; set; }
    public string Name { get; set; }
}

public class ArchivedGoalResponse
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string? Notes { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime LastModified { get; set; }
}

public class AllRemindersResponse
{
    public List<UserReminderDto> GeneralReminders { get; set; }
    public List<UserHabitReminderDto> UserHabitReminders { get; set; }
}

public class SavedReminderResponse
{
    public long Id { get; set; }
    public int UserNotificationRequestId { get; set; }
}
