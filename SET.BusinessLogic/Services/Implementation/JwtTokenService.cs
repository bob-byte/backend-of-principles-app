using Google.Apis.Auth;

using Microsoft.IdentityModel.Tokens;

using SET.Shared.Models;

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BusinessLogic;

public class JwtTokenService : IJwtTokenService
{
    private string _secret;
    private readonly IConfiguration m_configuration;

    public JwtTokenService( Func<string> secretFactory)
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

    public string GenerateJwtTokenForGoogleAuthorization( string userId )
    {
        string? jwtSecret = m_configuration[key: "JwtSettings:Secret"];
        if (string.IsNullOrWhiteSpace( jwtSecret ))
        {
            jwtSecret = m_configuration["PRINCIPLES_SERVER_JWT_SECRET"];

            if (string.IsNullOrWhiteSpace( jwtSecret ))
            {
                throw new InvalidOperationException( "JWT secret is not set" );
            }
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        byte[] key = Encoding.ASCII.GetBytes( jwtSecret );
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity( new Claim[]
            {
                new Claim(ClaimTypes.Name, userId)
            } ),
            Expires = DateTime.MaxValue,//TODO: implement token refresh
            SigningCredentials = new SigningCredentials( new SymmetricSecurityKey( key ), SecurityAlgorithms.HmacSha256Signature )
        };
        SecurityToken token = tokenHandler.CreateToken( tokenDescriptor );
        return tokenHandler.WriteToken( token );
    }
}
