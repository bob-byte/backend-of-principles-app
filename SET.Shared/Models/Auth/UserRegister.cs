namespace SET.Shared.Models.Auth;

public class UserRegister
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public Gender Gender { get; set; }
    public string? MainSlogan { get; set; }
    public string? Mission { get; set; }
}
