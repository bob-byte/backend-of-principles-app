namespace BusinessLogic.Models;

public class UserNewPassword
{
    public string Email { get; set; }
    public string NewPassword { get; set; }

    /// <summary>6-digit code from <c>GET api/account/code</c>.</summary>
    public int? Code { get; set; }
}
