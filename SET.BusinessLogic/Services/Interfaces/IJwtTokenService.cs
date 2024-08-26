
namespace BusinessLogic;

public interface IJwtTokenService
{
    string GetToken( User user );
    long GetUserIdFromJwt( string token );
}