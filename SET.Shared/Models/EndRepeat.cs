using System;
using System.Collections.Generic;

namespace SET.Shared.Models;

public class EndRepeat
{
    public Guid Id { get; set; }
    public bool? Never { get; set; }
    public DateTime? Date { get; set; }
    /// <summary>
    /// Times count to repeat
    /// </summary>
    public int? Timer { get; set; }

    public ICollection<NoticeRepeat> Repeats { get; set; }
}