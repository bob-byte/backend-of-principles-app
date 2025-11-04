namespace SET.WebAPI.Models;

public record HabitComplexityDto
{
    public long HabitId { get; set; }
    public int Complexity { get; set; }
}
