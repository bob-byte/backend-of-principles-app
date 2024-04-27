using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SET.Shared.Models;

public class UserFrequency
{
    public Guid Id { get; set; }
    /// <summary>
    /// For example: if Frequency = Frequency.Weekly,and Interval = 2, then Notice will be made every 2 week.
    /// </summary>
    [Required]
    public int Interval { get; set; }
    [Required]
    public TimeSpan Frequency { get; set; }

    public ICollection<NoticeRepeat> Repeats { get; set; }
}