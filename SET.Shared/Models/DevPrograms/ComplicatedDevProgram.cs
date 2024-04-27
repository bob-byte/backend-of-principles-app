
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SET.Shared.Models;

public class ComplicatedDevProgram
{
    public Guid Id
    {
        get; set;
    }

    public string Name
    {
        get; set;
    }

    public TypeOfComplicatedDevProgram Type { get; set; }

    public string ShortDescription
    {
        get; set;
    }

    public string? FullDescription
    {
        get; set;
    }

    //public ICollection<UserDevelopmentProgram> DevelopmentPrograms;
}
