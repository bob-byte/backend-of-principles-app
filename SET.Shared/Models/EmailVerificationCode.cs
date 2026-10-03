using System;

namespace SET.Shared.Models;

/// <summary>One outstanding emailed code per email + purpose (signup / password reset).</summary>
public class EmailVerificationCode
{
    public long Id { get; set; }

    /// <summary>Lowercase trimmed email.</summary>
    public string Email { get; set; }

    /// <summary>See <see cref="EmailVerificationPurposes"/>.</summary>
    public string Purpose { get; set; }

    /// <summary>SHA-256 hex of the 6-digit code.</summary>
    public string CodeHash { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }
}

public static class EmailVerificationPurposes
{
    public const string Signup = "Signup";
    public const string PasswordReset = "PasswordReset";
}
