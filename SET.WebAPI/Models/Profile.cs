using System;

namespace SET.WebAPI.Models;

public class Profile
{
    public long Id { get; set; }
    public string Name { get; set; }

    public string MainSlogan { get; set; }

    public string Mission { get; set; }
    public string Email { get; set; }

    public Gender Gender { get; set; }

    public bool HasSeenRoadGuide { get; set; }

    public DateTime LastModified { get; set; }
}
