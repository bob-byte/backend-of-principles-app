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

    public string GenerateJwtTokenForGoogleAuthorization( GoogleJsonWebSignature.Payload payload )
    {
        var claims = new List<Claim>
           {
               new Claim(JwtRegisteredClaimNames.Email, payload.Email),
               new Claim(JwtRegisteredClaimNames.Name, payload.Name)
           };

        var key = new SymmetricSecurityKey( Encoding.UTF8.GetBytes( m_configuration["JwtSettings:Secret"] ) );
        var creds = new SigningCredentials( key, SecurityAlgorithms.HmacSha256 );

        var token = new JwtSecurityToken(
            issuer: m_configuration["JwtSettings:Issuer"],
            audience: m_configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.MaxValue,
            signingCredentials: creds );

        return new JwtSecurityTokenHandler().WriteToken( token );
    }
}
