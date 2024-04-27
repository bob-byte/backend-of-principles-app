using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace SET.Shared.Models;

/// <summary>
/// Each record is specific to a concrete user, unlike other RD71 tasks
/// </summary>
public class Reading
{
    public Guid Id { get; set; }

    public Rd71 Rd71 { get; set; }
    [InverseProperty("Reading")]
    public Book ReadingBook { get; set; }
    [InverseProperty("ReadingFinished")]
    public ICollection<Book> ReadBooksDuringChallenge { get; set; }
}