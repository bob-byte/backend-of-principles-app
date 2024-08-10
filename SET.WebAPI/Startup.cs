using BusinessLogic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SET.WebAPI.Extensions;
using Newtonsoft.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.IdentityModel.Tokens;

namespace SET.WebAPI;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        Log.Information( "Start of Startup.ConfigureServices" );

        services.AddDataProtection().UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration()
        {
            EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
            ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
        } );

        services.
            AddControllers().
            AddNewtonsoftJson( options =>
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore );

        services.AddJwtAuthentication(secretFactory: () =>
        {
            string? jwtSecret = Configuration[key: "JwtSettings:Secret"];
            if (string.IsNullOrWhiteSpace( jwtSecret ))
            {
                jwtSecret = Configuration[ "PRINCIPLES_SERVER_JWT_SECRET" ];

                if(string.IsNullOrWhiteSpace( jwtSecret ))
                {
                    throw new InvalidOperationException( "JWT secret is not set" );
                }
            }

            return jwtSecret!;
        });

        services.AddSwaggerWithBearer();
        services.AddAutoMapper();
        services.AddDbContext<AppDbContext>(options =>
        {
            string? connectionString;
#if DEBUG
            connectionString = Configuration.GetConnectionString( name: "DefaultConnection" );
#else
            connectionString = Configuration.GetConnectionString( "Hostinger" );
#endif
            options.UseNpgsql( connectionString );
        });
        services.AddScoped<IAuthService, AuthService>();

        Log.Information( "End of Startup.ConfigureServices" );
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        Log.Information( "Start of Startup.Configure" );

        app.UseSwagger();
        app.UseSwaggerUI( setupAction: opts => opts.SwaggerEndpoint( url: "/swagger/v1/swagger.json", name: "Principles.WebAPI v1" ) );
        
        app.UseDeveloperExceptionPage();

        app.UseRouting();

        app.UseHttpsRedirection();

        app.UseForwardedHeaders();

        app.UseAuthentication();

        app.UseAuthorization();

        app.UseEndpoints( endpoints => endpoints.MapControllers() );

        using IServiceScope scope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope();
        using AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Database.Migrate();

        Log.Information( "End of Startup.Configure" );
    }
}
