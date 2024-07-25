using Google.Apis.Auth;

using SET.Shared.Models;

namespace BusinessLogic;

public interface IJwtTokenService
{
    string GetToken( User user );
    string GenerateJwtTokenForGoogleAuthorization( GoogleJsonWebSignature.Payload payload );
}