using SET.Shared.Models;
using SET.Shared.Models.Auth;
using System.Threading.Tasks;

namespace BusinessLogic;

public interface IAuthService
{
    Task<User> RegisterAsync( UserRegister userRegister );
    Task<(User? foundUser, string? errorMsg)> LoginAsync( UserLogin userLogin );
    Task<GoogleAuthResponse> GoogleAuthAsync( string idToken, string accessToken );
}
