using System;

namespace SET.WebAPI.Models;

public class Profile
{
    public string Name { get; set; }

    public string MainSlogan { get; set; }

    public string Mission { get; set; }

    public Gender Gender { get; set; }

    public UserReminder? GeneralReminder { get; set; }
}
