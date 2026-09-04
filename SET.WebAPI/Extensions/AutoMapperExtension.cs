using AutoMapper;

using Microsoft.Extensions.DependencyInjection;

using SET.Shared.Models;
using SET.WebAPI.Models;

using System.Text.Json;

namespace SET.WebAPI.Extensions;

public static class AutoMapperExtension
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static IServiceCollection AddAutoMapper(this IServiceCollection services)
    {
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, Models.Profile>();
            cfg.CreateMap<UserAreaOfLifeUserHabit, DtoWithId>().ForMember( destinationMember: dest => dest.Id, memberOptions: opt => opt.MapFrom( src => src.AreaOfLifeId ) );
            cfg.CreateMap<SaveLogRequest, ClientLog>();
            cfg.CreateMap<Frequency, UserHabitInProgressShortDto.FrequencyDto>();
            cfg.CreateMap<ProgressOfHabit, ProgressOfHabitDto>();
            cfg.CreateMap<UserHabit, UserHabitInProgressShortDto>();
            cfg.CreateMap<UserHabit, UserHabitInProgressShortDto>().ForMember( u => u.AreasOfLife, opt => opt.Ignore() ).ForMember( u => u.Reminders, opt => opt.Ignore() );
            cfg.CreateMap<Frequency, EditUserHabitDto.FrequencyDto>();
            cfg.CreateMap<EditUserHabitDto.FrequencyDto, Frequency>();
            cfg.CreateMap<UserAreaOfLife, UserAreaOfLifeDto>();
            cfg.CreateMap<UserHabit, EditUserHabitDto>().ForMember( u => u.AreasOfLife, opt => opt.Ignore() ).ForMember( u => u.Reminders, opt => opt.Ignore() );
            cfg.CreateMap<EditUserHabitDto, UserHabit>().ForMember( u => u.AreasOfLife, opt => opt.Ignore() ).ForMember( u => u.Reminders, opt => opt.Ignore() );
            cfg.CreateMap<UpdateProgressDto, ProgressOfHabit>();
            cfg.CreateMap<UserGoal, UserGoalDto>();
            cfg.CreateMap<UserGoalDto, UserGoal>();
            cfg.CreateMap<UserReminder, UserReminderDto>();
            cfg.CreateMap<UserReminderDto, UserReminder>();
            cfg.CreateMap<UserHabitReminder, UserHabitReminderDto>()
                .ForMember( dest => dest.Offsets, opt => opt.MapFrom( src => DeserializeOffsets( src.OffsetsJson ) ) );
            cfg.CreateMap<UserHabitReminderDto, UserHabitReminder>()
                .ForMember( dest => dest.OffsetsJson, opt => opt.MapFrom( src => SerializeOffsets( src.Offsets ) ) )
                .ForMember( dest => dest.Description, opt => opt.NullSubstitute( string.Empty ) )
                .ForMember( dest => dest.Title, opt => opt.NullSubstitute( string.Empty ) );
            cfg.CreateMap<WeekDay, WeekDayDto>();
            cfg.CreateMap<WeekDayDto, WeekDay>();
        } );

        IMapper mapper = mapperConfig.CreateMapper();
        
        services.AddSingleton(mapper);

        return services;
    }

    private static List<ReminderOffsetDto> DeserializeOffsets( string? json )
    {
        if (string.IsNullOrWhiteSpace( json ))
        {
            return new List<ReminderOffsetDto>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<ReminderOffsetDto>>( json, JsonOptions )
                   ?? new List<ReminderOffsetDto>();
        }
        catch
        {
            return new List<ReminderOffsetDto>();
        }
    }

    private static string? SerializeOffsets( List<ReminderOffsetDto>? offsets )
    {
        if (offsets is null || offsets.Count == 0)
        {
            return null;
        }

        return JsonSerializer.Serialize( offsets, JsonOptions );
    }
}
