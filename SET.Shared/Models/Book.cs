using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SET.Shared.Models;

/// <summary>
/// Avoid circle relationships
/// </summary>
public class Book
{
    public Guid Id { get; set; }
    [Required]
    public string Name { get; set; }
    [Required]
    public string Author { get; set; }
    /// <summary>
    /// Developing books shoudn't be read 1 time, because user will not remember enough
    /// </summary>
    public int? TimesRead { get; set; }

    public Guid? ReadingId { get; set; }
    public Reading Reading { get; set; }
    public ICollection<Reading> ReadingFinished { get; set; }
}