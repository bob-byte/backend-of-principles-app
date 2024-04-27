using SET.Shared.Models;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SET.Shared.Models;

public class BuiltInFrequency
{
    public Guid Id { get; set; }
    [Required]
    public FrequencyType Frequency { get; set; }

    public ICollection<NoticeRepeat> Repeats { get; set; }
}