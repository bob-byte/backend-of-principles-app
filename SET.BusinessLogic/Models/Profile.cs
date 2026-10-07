using System;

namespace BusinessLogic.Models;

public class Profile
{
    public long Id { get; set; }
    public string Name { get; set; }

    public string MainSlogan { get; set; }

    public string Mission { get; set; }
    public string Email { get; set; }

    public Gender Gender { get; set; }

    public bool HasSeenRoadGuide { get; set; }

    /// <summary>UTC date-only of the latest app open across the user's devices.</summary>
    public DateTime? LastAppOpen { get; set; }

    public DateTime LastModified { get; set; }
}
