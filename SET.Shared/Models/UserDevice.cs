using System;

namespace SET.Shared.Models;

/// <summary>
/// An app install that receives silent sync pushes (FCM token; FCM relays to APNs on Apple).
/// </summary>
public class UserDevice
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public User User { get; set; }

    /// <summary>Random per-install id the client also sends as <c>X-Device-Id</c>.</summary>
    public string DeviceId { get; set; }

    public string PushToken { get; set; }

    /// <summary><c>android</c>, <c>ios</c>, or <c>macos</c>.</summary>
    public string Platform { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
