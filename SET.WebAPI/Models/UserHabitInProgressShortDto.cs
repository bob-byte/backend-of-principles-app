using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET.WebAPI.Models;

public class UserHabitInProgressShortDto
{
    public class FrequencyDto
    {
        public long Id { get; set; }
        public FrequencyType Type { get; set; }
        public int Repeats { get; set; }
        public int IntervalLengthInDays { get; set; }
    }
    public class ProgressOfHabitDto
    {
        public long Id { get; set; }
        public DateOnly Date { get; set; }
        public int Value { get; set; }
    }

    public long Id { get; set; }
    public string Name { get; set; }
    public IEnumerable<ProgressOfHabitDto> Progresses { get; set; }
    public FrequencyDto Frequency { get; set; }
    public int Complexity { get; set; }
    public int Priority { get; set; }
}
