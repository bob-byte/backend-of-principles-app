using AutoMapper;
using BusinessLogic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SET.DataAccess;
using SET.UnitTests.TestSupport;

namespace SET.UnitTests;

public class BusinessLogicServiceCollectionExtensionsTests
{
    public static TheoryData<Type> ServiceTypes => new()
    {
        typeof( IMapper ),
        typeof( IAuthService ),
        typeof( IAccountService ),
        typeof( IEmailSender ),
        typeof( IProfileService ),
        typeof( IAreaOfLifeService ),
        typeof( IGoalService ),
        typeof( IHabitService ),
        typeof( IHabitProgressService ),
        typeof( IReminderService ),
        typeof( ITaskService ),
        typeof( ISyncService ),
        typeof( IDeviceService ),
        typeof( IClientLogService ),
        typeof( IAiService ),
        typeof( IAiAssistantService ),
        typeof( IAiConversationService ),
    };

    /// <summary>Mirrors the infrastructure <c>Startup</c> registers next to <c>AddBusinessLogic()</c>.</summary>
    private static ServiceProvider BuildProvider()
    {
        ServiceCollection services = new();
        services.AddDbContext<AppDbContext>( o => o
            .UseInMemoryDatabase( Guid.NewGuid().ToString() )
            .ConfigureWarnings( w => w.Ignore( InMemoryEventId.TransactionIgnoredWarning ) ) );
        services.AddSingleton<IConfiguration>( new ConfigurationBuilder().Build() );
        services.AddSingleton( Mock.Of<IJwtTokenService>() );
        services.AddSingleton( Mock.Of<IChatClient>() );
        services.AddSingleton<ISyncPushService>( new RecordingSyncPushService() );
        services.AddBusinessLogic();
        return services.BuildServiceProvider( new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true } );
    }

    [Theory]
    [MemberData( nameof( ServiceTypes ) )]
    public void AddBusinessLogic_RequestScope_ResolvesService( Type serviceType )
    {
        using ServiceProvider provider = BuildProvider();
        using IServiceScope scope = provider.CreateScope();

        Assert.NotNull( scope.ServiceProvider.GetRequiredService( serviceType ) );
    }
}
