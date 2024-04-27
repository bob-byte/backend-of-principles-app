using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET.Shared.Models;

public enum TypeOfConfiguredDevProgramEnum
{
    /// <summary>
    /// We don't show for users this type
    /// </summary>
    BuiltIn,
    FullFreedom,

    /// <summary>
    /// Focus only on 1 habit per life area
    /// </summary>
    ExtremeFocus
}

public class TypeOfConfiguredDevProgram
{
    public Guid Id { get; set; }

    public TypeOfConfiguredDevProgramEnum Name { get; set; }

    public bool IsDisabled { get; set; }
}
