using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SET.Shared.Models;

public class Reminder
{
    /// <summary>
    /// Not Guid type, because reminder has fixed coun
    /// </summary>
    public Guid Id { get; set; }
    [Required]
    public TimeSpan TimeToMainNotice { get; set; }

    public ICollection<Notice> Notices { get; set; }
}