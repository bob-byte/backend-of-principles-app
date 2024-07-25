using BusinessLogic;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

using System;
using System.Text;

namespace SET.WebAPI.Extensions;

public static class JwtAuthenticationExtension
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, Func<string> secretFactory, IConfiguration configuration)
    {
        string secret = secretFactory();

        services.AddAuthentication(x =>
        {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(c =>
        {
            c.SaveToken = true;
            c.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret)),
                ValidateIssuer = true,
                ValidateAudience = true,
                RequireExpirationTime = false,
                ValidateLifetime = true,
                ValidIssuer = configuration["JwtSettings:Issuer"],
                ValidAudience = configuration["JwtSettings:Audience"],
            };
        }).AddGoogle( options =>
        {
            options.ClientId = configuration["Google:ClientId"];
            options.ClientSecret = configuration["Google:ClientSecret"];
            options.Scope.Add( "https://www.googleapis.com/auth/user.gender.read" );
        } );

        services.AddSingleton(typeof(IJwtTokenService), new JwtTokenService(() => secretFactory()));
        return services;
    }
}
