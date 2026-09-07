using System;
using System.Collections.Generic;
using System.Linq;
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
            Subtasks = ( entity.Subtasks ?? Array.Empty<TaskSubtask>() )
                .OrderBy( s => s.SortOrder )
                .Select( ToSubtaskDto )
                .ToList(),
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
        ApplySubtasks( entity, request.Subtasks );
    }

    public static void ApplySubtasks( TaskEntity entity, List<TaskSubtaskDto>? subtasks )
    {
        // Null means an older client omitted the field — keep existing rows.
        if (subtasks is null)
        {
            return;
        }

        entity.Subtasks ??= new List<TaskSubtask>();
        entity.Subtasks.Clear();

        var order = 0;
        foreach (TaskSubtaskDto item in subtasks)
        {
            string title = ( item.Name ?? string.Empty ).Trim();
            if (string.IsNullOrEmpty( title ))
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

            if (title.Length > 255)
            {
                title = title[ ..255 ];
            }

            entity.Subtasks.Add( new TaskSubtask
            {
                ClientId = clientId,
                Title = title,
                IsCompleted = item.IsCompleted,
                SortOrder = order++,
            } );
        }
    }

    public static TaskSubtaskDto ToSubtaskDto( TaskSubtask entity )
    {
        return new TaskSubtaskDto
        {
            Id = entity.ClientId,
            Name = entity.Title,
            IsCompleted = entity.IsCompleted,
            SortOrder = entity.SortOrder,
        };
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
