using System;
using System.ComponentModel.DataAnnotations;

namespace SET.Shared.Models;

public class TimeZone
{
    public Guid Id { get; set; }
    [Required]
    public string Zone { get; set; }

    public Guid? NoticeId { get; set; }
    public Notice Notice { get; set; }
}