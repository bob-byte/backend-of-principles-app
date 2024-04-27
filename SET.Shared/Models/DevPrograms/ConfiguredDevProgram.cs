
using SET.Shared.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SET.Shared.Models;

public class ConfiguredDevProgram
{
    public Guid Id
    {
        get; set;
    }

    public string Name
    {
        get; set;
    }

    //public TypeOfConfiguredDevProgram Type { get; set; }

    public int MaxTotalHabits { get; set; }

    public int MaxHabitsPerAreaOfLife
    {
        get; set;
    }

    public int MaxNewTotalHabitsToAchieve
    {
        get; set;
    }

    public int MaxNewHabitsPerAreaOfLifeToAchieve
    {
        get; set;
    }

    public int MaxSavedStatements
    {
        get; set;
    }
}