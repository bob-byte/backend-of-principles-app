
using Microsoft.IdentityModel.Tokens;

using SET.Shared.Models;

using System;
using System.IdentityModel.Tokens.Jwt;
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
            Expires = DateTime.Today.AddDays(7),//TODO: implement token refresh
            SigningCredentials = new SigningCredentials( new SymmetricSecurityKey( key ), SecurityAlgorithms.HmacSha256Signature )
        };

        SecurityToken token = tokenHandler.CreateToken( tokenDescriptor );
        return tokenHandler.WriteToken( token );
    }
}
