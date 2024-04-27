using Microsoft.IdentityModel.Tokens;

using SET.Shared.Models;

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BusinessLogic;

public class JwtTokenService : IJwtTokenService
{
    private string _secret;

    public JwtTokenService( Func<string> secretFactory )
    {
        _secret = secretFactory?.Invoke() ?? throw new ArgumentNullException( nameof( secretFactory ) );
    }

    public string GetToken( User user )
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        byte[] key = Encoding.ASCII.GetBytes( _secret );
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity( new Claim[]
            {
                new Claim(ClaimTypes.Name, user.Id.ToString())
            } ),
            Expires = DateTime.MaxValue,//TODO: implement token refresh
            SigningCredentials = new SigningCredentials( new SymmetricSecurityKey( key ), SecurityAlgorithms.HmacSha256Signature )
        };
        SecurityToken token = tokenHandler.CreateToken( tokenDescriptor );
        return tokenHandler.WriteToken( token );
    }
}
