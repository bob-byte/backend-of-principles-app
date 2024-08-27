
using Microsoft.IdentityModel.Tokens;

using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Text;

namespace BusinessLogic;

public class JwtTokenService : IJwtTokenService
{
    private readonly string m_secret;

    public JwtTokenService( Func<string> secretFactory)
    {
        m_secret = secretFactory?.Invoke() ?? throw new ArgumentNullException( nameof( secretFactory ) );
    }

    public string GetToken( User user )
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        byte[] key = Encoding.ASCII.GetBytes( m_secret );
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity( new Claim[]
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Role, "FreeAccount")
            } ),
            Expires = DateTime.Today.AddDays(7),
            SigningCredentials = new SigningCredentials( new SymmetricSecurityKey( key ), SecurityAlgorithms.HmacSha256Signature )
        };

        SecurityToken token = tokenHandler.CreateToken( tokenDescriptor );
        return tokenHandler.WriteToken( token );
    }

    public long GetUserIdFromJwt( string token )
    {
        if (string.IsNullOrWhiteSpace( token ))
        {
            throw new HttpRequestException( "AccessTokenIsNullOrWhitespace" );
        }

        JwtSecurityTokenHandler handler = new();
        JwtSecurityToken jsonToken = handler.ReadToken( token ) as JwtSecurityToken
            ?? throw new HttpRequestException( "JwtSecurityTokenIsNull" );

        string? userIdAsStr = jsonToken.Claims.FirstOrDefault( c => c.Type == "nameid" )?.Value;
        bool isParsedUserId = long.TryParse( userIdAsStr, out long userId );
        if (!isParsedUserId)
        {
            userIdAsStr = jsonToken?.Claims.FirstOrDefault( c => c.Type == "name" )?.Value;
            isParsedUserId = long.TryParse( userIdAsStr, out userId );
            if (!isParsedUserId)
            {
                throw new HttpRequestException( "UserIdIsNotInAccessToken" );
            }
        }

        return userId;
    }
}