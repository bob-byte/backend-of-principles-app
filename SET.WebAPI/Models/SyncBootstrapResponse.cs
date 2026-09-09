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
    /// Cursor for the next GET /sync/changes?since= call.
    public DateTime ServerTime { get; set; }

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

/// Incremental catch-up for peers that already have a full bootstrap.
public class SyncChangesResponse
{
    public DateTime ServerTime { get; set; }

    /// Client should call GET /sync/bootstrap instead (since too old / missing).
    public bool RequiresFullBootstrap { get; set; }

    public SyncBootstrapUserDto? User { get; set; }
    public List<UserGoalDto> Goals { get; set; } = new();
    public List<UserHabitInProgressShortDto> ActiveHabits { get; set; } = new();
    public List<SyncBootstrapArchivedHabitDto> ArchivedHabits { get; set; } = new();
    public UserReminderDto? HabitsReportReminder { get; set; }
    public List<UserReminderDto> GeneralReminders { get; set; } = new();
    public List<UserHabitReminderDto> UserHabitReminders { get; set; } = new();
    public List<TaskItemDto> Tasks { get; set; } = new();
    public List<AiConversationDto> Conversations { get; set; } = new();

    public List<long> DeletedGoalIds { get; set; } = new();
    public List<long> DeletedHabitIds { get; set; } = new();
    public List<long> DeletedTaskIds { get; set; } = new();
    public List<long> DeletedConversationIds { get; set; } = new();
}
