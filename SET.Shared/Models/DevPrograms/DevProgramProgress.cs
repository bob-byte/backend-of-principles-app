
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SET.Shared.Models;

public class DevProgramProgress
{
    public Guid Id
    {
        get; set;
    }

    public decimal ChangedInPercent
    {
        get; set;
    }

    public decimal TotalHabitsProgress
    {
        get; set;
    }

    public decimal TotalProgressOfGoals
    {
        get; set;
    }
}