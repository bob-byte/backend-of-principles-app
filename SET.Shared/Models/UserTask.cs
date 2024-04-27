using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SET.Shared.Models;

/// <summary>
/// In this table is suggested task and inputted by user
/// </summary>
public class UserTask
{
    public Guid Id { get; set; }
    [Required]
    public string Name { get; set; }
    [Required]
    public ActivityArea ActivityArea { get; set; }

    public ICollection<Rd71> Rd71s { get; set; }
}