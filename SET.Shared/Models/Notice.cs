using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SET.Shared.Models;

/// <summary>
/// Analog of Google mobile event in calendar
/// </summary>
public class Notice
{
    public Guid Id { get; set; }
    public string Description { get; set; }
    [Required]
    public bool IsAllDay { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }

    public ICollection<Reminder> Reminders { get; set; }
    public TimeZone TimeZone { get; set; }
    public Guid? RepeatId { get; set; }
    public NoticeRepeat Repeat { get; set; }
    public Guid? GoalId { get; set; }
    public Goal Goal { get; set; }
}