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
            Subgoals = ( entity.Subgoals ?? Array.Empty<GoalSubgoal>() )
                .OrderBy( s => s.SortOrder )
                .Select( ToSubgoalDto )
                .ToList(),
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
        ApplySubgoals( entity, dto.Subgoals );
    }

    /// <summary>Null keeps existing rows. An empty list clears them.</summary>
    public static void ApplySubgoals( UserGoal entity, List<GoalSubgoalDto>? subgoals )
    {
        if (subgoals is null)
        {
            return;
        }

        entity.Subgoals ??= new List<GoalSubgoal>();
        entity.Subgoals.Clear();

        var order = 0;
        var seen = new HashSet<string>( StringComparer.Ordinal );
        foreach (GoalSubgoalDto item in subgoals)
        {
            string name = ( item.Name ?? string.Empty ).Trim();
            if (string.IsNullOrEmpty( name ))
            {
                continue;
            }

            string clientId = ( item.Id ?? string.Empty ).Trim();
            if (string.IsNullOrEmpty( clientId ))
            {
                clientId = Guid.NewGuid().ToString( "N" );
            }

            if (clientId.Length > 64)
            {
                clientId = clientId[ ..64 ];
            }

            if (!seen.Add( clientId ))
            {
                continue;
            }

            if (name.Length > 255)
            {
                name = name[ ..255 ];
            }

            entity.Subgoals.Add( new GoalSubgoal
            {
                ClientId = clientId,
                Name = name,
                IsCompleted = item.IsCompleted,
                SortOrder = order++,
            } );
        }
    }

    public static GoalSubgoalDto ToSubgoalDto( GoalSubgoal entity )
    {
        return new GoalSubgoalDto
        {
            Id = entity.ClientId,
            Name = entity.Name,
            IsCompleted = entity.IsCompleted,
            SortOrder = entity.SortOrder,
        };
    }
}
