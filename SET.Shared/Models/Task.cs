using System;

namespace SET.Shared.Models;

public class Task{
    public long Id { get; set; }
    public long UserId { get; set; }
    public User User { get; set; }
    public string Name {get; set;}
    public string? Notes {get; set;}
    public DateOnly? Date {get; set;}
    public TimeOnly? Time {get; set;}
}