namespace SET.Shared.Models.Auth;

public class UserRegister
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string? Password { get; set; }
    public Gender Gender { get; set; }
    public string? MainSlogan { get; set; }
    public string? Mission { get; set; }

    /// <summary>6-digit code from <c>GET api/account/signupcode</c> (required for production register).</summary>
    public int? Code { get; set; }
}
