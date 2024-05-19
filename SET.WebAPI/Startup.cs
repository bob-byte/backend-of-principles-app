using BusinessLogic;
using SET.DataAccess;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SET.WebAPI.Extensions;
using SET.Shared.Services.Implementation;
using SET.Shared.Services.Interfaces;
using System;
using Newtonsoft.Json;

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
        services.
            AddControllers().
            AddNewtonsoftJson( options =>
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore );

        services.AddJwtAuthentication(() => Configuration[ key: "JwtSettings:Secret" ]);
        services.AddSwaggerWithBearer();
        services.AddAutoMapper();
        services.AddDbContext<AppDbContext>(options =>
        {
            string? connectionString;
#if DEBUG
            connectionString = Configuration.GetConnectionString( name: "DefaultConnection" );
#else
            connectionString = Configuration.GetConnectionString( "Azure" );
#endif
            options.UseNpgsql( connectionString );
        });
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IFileSystemService, FileSystemService>();
        services.AddSingleton<IRandomService, RandomService>();
        services.AddSingleton<IProgressOfHabitService, ProgressOfHabitService>();
        services.AddSingleton<IServiceOfHabit, ServiceOfHabit>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseSwagger();
        app.UseSwaggerUI( setupAction: opts => opts.SwaggerEndpoint( url: "/swagger/v1/swagger.json", name: "Principles.WebAPI v1" ) );
        
        app.UseDeveloperExceptionPage();

        app.UseForwardedHeaders();

        app.UseRouting();

        app.UseAuthentication();

        app.UseAuthorization();

        app.UseEndpoints( endpoints => endpoints.MapControllers() );

        using IServiceScope scope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope();
        using AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Database.Migrate();
    }
}
