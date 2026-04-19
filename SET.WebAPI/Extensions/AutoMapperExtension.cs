using AutoMapper;

using Microsoft.Extensions.DependencyInjection;

using SET.Shared.Models;

namespace SET.WebAPI.Extensions;

public static class AutoMapperExtension
{
    public static IServiceCollection AddAutoMapper( this IServiceCollection services )
    {
        var mapperConfig = new MapperConfiguration( cfg =>
        {
            cfg.CreateMap<User, Models.Profile>()
                .ForMember( dest => dest.LastModified, opt => opt.MapFrom( src => src.UpdatedAt ) );
            cfg.CreateMap<UserAreaOfLifeUserHabit, DtoWithId>().ForMember( destinationMember: dest => dest.Id, memberOptions: opt => opt.MapFrom( src => src.AreaOfLifeId ) );
            cfg.CreateMap<SaveLogRequest, ClientLog>();
            cfg.CreateMap<Frequency, UserHabitInProgressShortDto.FrequencyDto>()
                .ForMember( dest => dest.LastModified, opt => opt.Ignore() );
            cfg.CreateMap<ProgressOfHabit, ProgressOfHabitDto>()
                .ForMember( dest => dest.LastModified, opt => opt.MapFrom( src => src.UpdatedAt ) );
            cfg.CreateMap<UserHabit, UserHabitInProgressShortDto>()
                .ForMember( dest => dest.LastModified, opt => opt.MapFrom( src => src.UpdatedAt ?? src.CreatedAt ) );
            cfg.CreateMap<UserHabit, UserHabitInProgressShortDto>().ForMember( u => u.AreasOfLife, opt => opt.Ignore() ).ForMember( u => u.Reminders, opt => opt.Ignore() );
            cfg.CreateMap<Frequency, EditUserHabitDto.FrequencyDto>()
                .ForMember( dest => dest.LastModified, opt => opt.Ignore() );
            cfg.CreateMap<EditUserHabitDto.FrequencyDto, Frequency>();
            cfg.CreateMap<UserAreaOfLife, UserAreaOfLifeDto>();
            cfg.CreateMap<UserHabit, EditUserHabitDto>()
                .ForMember( u => u.AreasOfLife, opt => opt.Ignore() )
                .ForMember( u => u.Reminders, opt => opt.Ignore() )
                .ForMember( dest => dest.LastModified, opt => opt.MapFrom( src => src.UpdatedAt ?? src.CreatedAt ) );
            cfg.CreateMap<EditUserHabitDto, UserHabit>().ForMember( u => u.AreasOfLife, opt => opt.Ignore() ).ForMember( u => u.Reminders, opt => opt.Ignore() );
            cfg.CreateMap<UpdateProgressDto, ProgressOfHabit>();
            cfg.CreateMap<UserGoal, UserGoalDto>()
                .ForMember( dest => dest.LastModified, opt => opt.MapFrom( src => src.UpdatedAt ?? src.CreatedAt ) );
            cfg.CreateMap<UserGoalDto, UserGoal>();
            cfg.CreateMap<UserReminder, UserReminderDto>()
                .ForMember( dest => dest.LastModified, opt => opt.MapFrom( src => src.UpdatedAt ) );
            cfg.CreateMap<UserReminderDto, UserReminder>();
            cfg.CreateMap<UserHabitReminder, UserHabitReminderDto>()
                .ForMember( dest => dest.LastModified, opt => opt.Ignore() );
            cfg.CreateMap<UserHabitReminderDto, UserHabitReminder>();
            cfg.CreateMap<WeekDay, WeekDayDto>()
                .ForMember( dest => dest.LastModified, opt => opt.Ignore() );
            cfg.CreateMap<WeekDayDto, WeekDay>();
        } );

        IMapper mapper = mapperConfig.CreateMapper();

        services.AddSingleton( mapper );

        return services;
    }
}
