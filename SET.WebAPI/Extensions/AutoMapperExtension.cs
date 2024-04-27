using AutoMapper;

using Microsoft.Extensions.DependencyInjection;

using SET.Shared.Models;

namespace SET.WebAPI.Extensions;

public static class AutoMapperExtension
{
    public static IServiceCollection AddAutoMapper(this IServiceCollection services)
    {
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<GoalDto, Goal>();
            cfg.CreateMap<FileEntityDto, FileEntity>();
            cfg.CreateMap<ChallengeDto, Challenge>();
            cfg.CreateMap<User, Models.Profile>();
            cfg.CreateMap<UserHabit, UserHabitInProgressShortDto>().ForMember( u => u.AreasOfLife, opt => opt.Ignore() );
            cfg.CreateMap<UserHabit, EditUserHabitDto>().ForMember( u => u.AreasOfLife, opt => opt.Ignore() );
            cfg.CreateMap<EditUserHabitDto, UserHabit>().ForMember( u => u.AreasOfLife, opt => opt.Ignore() );
            cfg.CreateMap<UpdateProgressDto, ProgressOfHabit>();
        } );

        IMapper mapper = mapperConfig.CreateMapper();
        
        services.AddSingleton(mapper);

        return services;
    }
}
