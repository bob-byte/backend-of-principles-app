using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET.Shared.Models;

public class Frequency
{
    public long Id { get; set; }
    public FrequencyType Type { get; set; }
    public double Value { get; set; }
    public int Repeats { get; set; }
    public int IntervalLengthInDays { get; set; }
    public ICollection<UserHabit> Habits { get; set; }

    public IntervalType IntervalType()
    {
        var result = (IntervalType)IntervalLengthInDays;
        if (result != Models.IntervalType.Day && result != Models.IntervalType.Week && result != Models.IntervalType.Month && result != Models.IntervalType.Year)
        {
            result = Models.IntervalType.Other;
        }
        return result;
    }
}

public enum IntervalType
{
    Day = 1,
    Week = 7,
    Month = 30,
    Year = 365,
    //for example, every 2 days
    Other = 0
}