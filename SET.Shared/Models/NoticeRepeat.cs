using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SET.Shared.Models;

public class NoticeRepeat
{
    public Guid Id { get; set; }

    public ICollection<Notice> Notices { get; set; }
    /// <summary>
    /// If it is null, UserFrequency must has value
    /// </summary>
    public Guid? BuiltInFrequencyId { get; set; }
    public BuiltInFrequency BuiltInFrequency { get; set; }
    /// <summary>
    /// If it is null, Frequency must has value
    /// </summary>
    public Guid? UserFrequencyId { get; set; }
    public UserFrequency UserFrequency { get; set; }
    [Required]
    public Guid EndRepeatId { get; set; }
    public EndRepeat EndRepeat { get; set; }
}