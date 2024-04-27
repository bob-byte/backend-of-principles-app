using System;
using System.ComponentModel.DataAnnotations;

namespace SET.Shared.Models;

public class RecomendedBook
{
    public Guid Id { get; set; }
    [Required]
    public string Name { get; set; }
    [Required]
    public string Author { get; set; }
}
