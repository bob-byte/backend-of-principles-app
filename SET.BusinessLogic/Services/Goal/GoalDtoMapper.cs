using BusinessLogic.Models;
using SET.Shared.Models;

namespace BusinessLogic;

public static class GoalDtoMapper
{
    public static UserGoalDto ToDto( UserGoal entity )
    {
        return new UserGoalDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Notes = entity.Notes,
            IsCompleted = entity.IsCompleted,
            IsArchived = entity.IsArchived,
            Deadline = entity.Deadline,
            DeadlineTime = entity.Deadline is null ? null : entity.DeadlineTime,
            Reminders = entity.Deadline is null
                ? new List<TaskReminderOffsetDto>()
                : TaskDtoMapper.ParseReminders( entity.RemindersJson ),
            LastModified = entity.UpdatedAt ?? entity.ArchivingTime ?? entity.CreatedAt,
        };
    }

    public static void ApplyDto( UserGoal entity, UserGoalDto dto )
    {
        entity.Name = dto.Name;
        entity.Notes = string.IsNullOrWhiteSpace( dto.Notes ) ? null : dto.Notes.Trim();
        entity.IsCompleted = dto.IsCompleted;
        entity.IsArchived = dto.IsArchived;
        entity.Deadline = dto.Deadline;
        entity.DeadlineTime = dto.Deadline is null ? null : dto.DeadlineTime;
        entity.RemindersJson = dto.Deadline is null
            ? null
            : TaskDtoMapper.SerializeReminders( dto.Reminders );
    }
}
