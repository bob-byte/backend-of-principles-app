using SET.Shared.Models;

namespace BusinessLogic;

public interface IJwtTokenService
{
    string GetToken( User user );
}