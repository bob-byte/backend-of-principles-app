namespace BusinessLogic;

public static class BusinessLogicServiceCollectionExtensions
{
    /// <summary>
    /// Registers the AutoMapper profile and scoped feature services. The host still registers
    /// infrastructure: <c>AppDbContext</c>, <c>IJwtTokenService</c>, <c>IChatClient</c>, and push senders.
    /// </summary>
    public static IServiceCollection AddBusinessLogic( this IServiceCollection services )
    {
        services.AddBusinessLogicMapper();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IEmailSender, EmailSender>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IAreaOfLifeService, AreaOfLifeService>();
        services.AddScoped<IGoalService, GoalService>();
        services.AddScoped<IHabitService, HabitService>();
        services.AddScoped<IHabitProgressService, HabitProgressService>();
        services.AddScoped<IReminderService, ReminderService>();
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<ISyncService, SyncService>();
        services.AddScoped<IDeviceService, DeviceService>();
        services.AddScoped<IClientLogService, ClientLogService>();
        services.AddScoped<IAiService, AiService>();
        services.AddScoped<IAiAssistantService, AiAssistantService>();
        services.AddScoped<IAiConversationService, AiConversationService>();

        return services;
    }
}
