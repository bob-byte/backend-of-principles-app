using System;

namespace SET.Shared.Models;

/// <summary>
/// Tombstone for hard-deleted sync entities so GET /sync/changes can tell peers what to drop.
/// </summary>
public class SyncDeletion
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public User User { get; set; }

    /// <summary>One of <see cref="SyncEntityTypes"/>.</summary>
    public string EntityType { get; set; }

    public long EntityId { get; set; }
    public DateTime DeletedAt { get; set; }
}

public static class SyncEntityTypes
{
    public const string Goal = "goal";
    public const string Habit = "habit";
    public const string Task = "task";
    public const string Conversation = "conversation";
}
