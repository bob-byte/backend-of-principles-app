using System;

namespace SET.Shared.Models;

/// <summary>
/// Opaque refresh credential for a user session. Only the SHA-256 hash is stored.
/// </summary>
public class RefreshToken
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public User User { get; set; } = null!;

    /// <summary>SHA-256 hex of the plaintext refresh token sent to clients.</summary>
    public string TokenHash { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    /// <summary>Hash of the token that replaced this one after rotation (reuse detection).</summary>
    public string? ReplacedByTokenHash { get; set; }

    /// <summary>Optional client <c>X-Device-Id</c> when the session was issued.</summary>
    public string? DeviceId { get; set; }
}
