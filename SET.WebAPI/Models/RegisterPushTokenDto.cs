namespace SET.WebAPI.Models;

public class RegisterPushTokenDto
{
    /// <summary>Per-install id; the same value the client sends as <c>X-Device-Id</c>.</summary>
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>FCM registration token.</summary>
    public string Token { get; set; } = string.Empty;

    public string? Platform { get; set; }
}
