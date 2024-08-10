
namespace BusinessLogic;

public interface IJwtTokenService
{
    string GetToken( User user );
}