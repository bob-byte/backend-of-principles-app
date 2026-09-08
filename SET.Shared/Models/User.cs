using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SET.Shared.Models;

public class User
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public byte[]? Password { get; set; }

    public Gender Gender { get; set; }

    public string? MainSlogan { get; set; }
    public string? Mission { get; set; }

    /// <summary>
    /// Whether the account has completed (or skipped) the in-app road guide at least once.
    /// Per-device auto-play is gated locally; this flag is the durable profile record.
    /// </summary>
    public bool HasSeenRoadGuide { get; set; }

    public UserReminder? HabitsReportReminder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public ICollection<UserAreaOfLife> AreasOfLife { get; set; }
    public ICollection<UserHabit> Habits { get; set; }
    public ICollection<ClientLog> ClientLogs { get; set; }
    public ICollection<UserGoal> Goals { get; set; }
    public ICollection<TrackingOfUserNotificationRequests> TrackingOfUserNotificationRequests { get; set; }
}
