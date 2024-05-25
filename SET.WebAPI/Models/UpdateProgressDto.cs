using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET.WebAPI.Models;

public class UpdateProgressDto
{
    public long Id { get; set; }
    public DateOnly Date { get; set; }
    public int Value { get; set; }
    public long HabitId { get; set; }
}
