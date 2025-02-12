using System;

namespace SET.WebAPI.Models;

public class Profile
{
    public string Name { get; set; }

    public string MainSlogan { get; set; }

    public string Mission { get; set; }
    public string Email { get; set; }

    public Gender Gender { get; set; }
}
