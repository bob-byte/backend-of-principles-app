using System.Text.Json;
using SET.Shared.Models;
using SET.WebAPI.Models;
using TaskEntity = SET.Shared.Models.Task;

namespace SET.WebAPI.Helpers;

public static class TaskDtoMapper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static TaskItemDto ToDto( TaskEntity entity )
    {
        return new TaskItemDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Notes = entity.Notes,
            Date = entity.Date,
            Time = entity.Time,
            EndDate = entity.EndDate,
            EndTime = entity.EndTime,
            AllDay = entity.AllDay,
            IsCompleted = entity.IsCompleted,
            ConstantReminder = entity.ConstantReminder,
            ConstantNotificationRequestId = entity.ConstantNotificationRequestId,
            Reminders = ParseReminders( entity.RemindersJson ),
            Repeat = ParseRepeat( entity.RepeatJson ),
        };
    }

    public static void ApplyDto( TaskEntity entity, TaskItemDto request )
    {
        entity.Name = request.Name;
        entity.Notes = request.Notes;
        entity.Date = request.Date;
        entity.Time = request.Time;
        entity.EndDate = request.EndDate;
        entity.EndTime = request.EndTime;
        entity.AllDay = request.AllDay;
        entity.ConstantReminder = request.ConstantReminder;
        entity.ConstantNotificationRequestId = request.ConstantNotificationRequestId;
        entity.RemindersJson = SerializeReminders( request.Reminders );
        entity.RepeatJson = SerializeRepeat( request.Repeat );
    }

    public static List<TaskReminderOffsetDto> ParseReminders( string? json )
    {
        if (string.IsNullOrWhiteSpace( json ))
        {
            return new List<TaskReminderOffsetDto>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<TaskReminderOffsetDto>>( json, JsonOptions )
                   ?? new List<TaskReminderOffsetDto>();
        }
        catch
        {
            return new List<TaskReminderOffsetDto>();
        }
    }

    public static TaskRepeatDto? ParseRepeat( string? json )
    {
        if (string.IsNullOrWhiteSpace( json ))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<TaskRepeatDto>( json, JsonOptions );
        }
        catch
        {
            return null;
        }
    }

    public static string? SerializeReminders( List<TaskReminderOffsetDto>? reminders )
    {
        if (reminders is null || reminders.Count == 0)
        {
            return null;
        }

        return JsonSerializer.Serialize( reminders, JsonOptions );
    }

    public static string? SerializeRepeat( TaskRepeatDto? repeat )
    {
        if (repeat is null || string.Equals( repeat.Preset, "none", StringComparison.OrdinalIgnoreCase ))
        {
            return null;
        }

        return JsonSerializer.Serialize( repeat, JsonOptions );
    }
}
