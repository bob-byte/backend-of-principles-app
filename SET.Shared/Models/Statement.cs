using System;

namespace SET.Shared.Models;

public class Statement
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Author { get; set; }
    public string Text { get; set; }
}
