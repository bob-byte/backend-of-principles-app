using BusinessLogic;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

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
                RequireExpirationTime = false,
                ValidateLifetime = false
            };
        });

        services.AddSingleton(typeof(IJwtTokenService), new JwtTokenService(() => secretFactory()));
        return services;
    }
}
