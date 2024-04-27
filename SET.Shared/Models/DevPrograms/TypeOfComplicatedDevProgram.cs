using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET.Shared.Models;

public enum TypeOfComplicatedDevProgramEnum
{
    GreekCatholic
}

public class TypeOfComplicatedDevProgram
{
    public Guid Id { get; set; }

    public TypeOfComplicatedDevProgramEnum Name { get; set; }

    public bool IsDisabled { get; set; }
}
