using BusinessLogic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SET.WebAPI.Extensions;
using SET.WebAPI.Helpers;
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
#if DEBUG
        services.AddSwaggerWithBearer();
#endif
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
        services.AddScoped<IEmailSender, EmailSender>();
        services.AddScoped<IReminderService, ReminderService>();
        services.AddHttpClient<IAiService, AiService>( client =>
        {
            string baseUrl = Configuration["OpenAi:BaseUrl"];
            if (string.IsNullOrWhiteSpace( baseUrl ))
            {
                baseUrl = "https://api.openai.com/v1/";
            }
            if (!baseUrl.EndsWith( '/' ))
            {
                baseUrl += "/";
            }

            client.BaseAddress = new Uri( baseUrl );
            client.Timeout = TimeSpan.FromSeconds( 120 );
        } );
        services.AddSingleton<FcmCredentials>();
        services.AddHttpClient<IPushSender, FcmPushSender>( client => client.Timeout = TimeSpan.FromSeconds( 15 ) );
        services.AddSingleton<SyncPushDispatcher>();
        services.AddSingleton<ISyncPushService>( sp => sp.GetRequiredService<SyncPushDispatcher>() );
        services.AddHostedService( sp => sp.GetRequiredService<SyncPushDispatcher>() );

        Log.Information( "End of Startup.ConfigureServices" );
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        Log.Information( "Start of Startup.Configure" );
#if DEBUG
        app.UseSwagger();
        app.UseSwaggerUI( setupAction: opts => opts.SwaggerEndpoint( url: "/swagger/v1/swagger.json", name: "Principles.WebAPI v1" ) );
#endif
        app.UseDeveloperExceptionPage();

        app.UseRouting();

        app.UseHttpsRedirection();

        app.UseForwardedHeaders();

        app.UseAuthentication();

#if !DEBUG
        app.UseMiddleware<RequestCallerLoggingMiddleware>();
#endif

        app.UseAuthorization();

        app.UseEndpoints( endpoints => endpoints.MapControllers() );

        using IServiceScope scope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope();
        using AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Database.Migrate();

        Log.Information( "End of Startup.Configure" );
    }
}
