using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET.Shared.Models;

public class UpdateProgressDto
{
    public Guid Id { get; set; }
    public DateOnly Date { get; set; }
    public int Value { get; set; }
    public Guid HabitId { get; set; }
    public double PercentageAchieved { get; set; }
}

