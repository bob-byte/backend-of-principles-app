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
            cfg.CreateMap<FileEntityDto, FileEntity>();
            cfg.CreateMap<User, Models.Profile>();
            cfg.CreateMap<UserAreaOfLifeUserHabit, DtoWithId>().ForMember( destinationMember: dest => dest.Id, memberOptions: opt => opt.MapFrom( src => src.AreaOfLifeId ) );
            cfg.CreateMap<Frequency, UserHabitInProgressShortDto.FrequencyDto>();
            cfg.CreateMap<ProgressOfHabit, UserHabitInProgressShortDto.ProgressOfHabitDto>();
            cfg.CreateMap<UserHabit, UserHabitInProgressShortDto>();
            cfg.CreateMap<Frequency, EditUserHabitDto.FrequencyDto>();
            cfg.CreateMap<EditUserHabitDto.FrequencyDto, Frequency>();
            cfg.CreateMap<UserAreaOfLife, EditUserHabitDto.UserAreaOfLifeDto>();
            cfg.CreateMap<UserHabit, EditUserHabitDto>().ForMember( u => u.AreasOfLife, opt => opt.Ignore() );
            cfg.CreateMap<EditUserHabitDto, UserHabit>().ForMember( u => u.AreasOfLife, opt => opt.Ignore() );
            cfg.CreateMap<UpdateProgressDto, ProgressOfHabit>();
        } );

        IMapper mapper = mapperConfig.CreateMapper();
        
        services.AddSingleton(mapper);

        return services;
    }
}
