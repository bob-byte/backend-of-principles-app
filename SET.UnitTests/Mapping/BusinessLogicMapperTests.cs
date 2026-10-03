using AutoMapper;
using BusinessLogic;
using BusinessLogic.Models;
using SET.Shared.Models;

namespace SET.UnitTests.Mapping;

public class BusinessLogicMapperTests
{
    private readonly IMapper m_mapper = BusinessLogicMapper.Create();

    [Fact]
    public void Map_HabitReminderOffsets_RoundTripThroughJson()
    {
        UserHabitReminderDto dto = new()
        {
            Title = "Read",
            Description = "Pages",
            DaysOfWeek = Array.Empty<WeekDayDto>(),
            Offsets = new List<ReminderOffsetDto> { new() { OffsetMinutes = 10, NotificationRequestId = 5 } },
        };

        UserHabitReminder entity = m_mapper.Map<UserHabitReminder>( dto )!;
        UserHabitReminderDto back = m_mapper.Map<UserHabitReminderDto>( entity )!;

        Assert.NotNull( entity.OffsetsJson );
        ReminderOffsetDto offset = Assert.Single( back.Offsets! );
        Assert.Equal( 10, offset.OffsetMinutes );
        Assert.Equal( 5, offset.NotificationRequestId );
    }

    [Fact]
    public void Map_HabitReminderNullTexts_BecomeEmptyAndNullOffsets()
    {
        UserHabitReminder entity = m_mapper.Map<UserHabitReminder>( new UserHabitReminderDto
        {
            DaysOfWeek = Array.Empty<WeekDayDto>(),
        } )!;

        Assert.Equal( string.Empty, entity.Title );
        Assert.Equal( string.Empty, entity.Description );
        Assert.Null( entity.OffsetsJson );
        Assert.Empty( m_mapper.Map<UserHabitReminderDto>( entity )!.Offsets! );
    }

    [Fact]
    public void Map_User_MapsToProfile()
    {
        BusinessLogic.Models.Profile profile = m_mapper.Map<BusinessLogic.Models.Profile>( new User
        {
            Name = "Ada",
            Email = "ada@example.com",
            MainSlogan = "Go",
            Mission = "Build",
            Gender = Gender.Woman,
            HasSeenRoadGuide = true,
        } )!;

        Assert.Equal( "Ada", profile.Name );
        Assert.Equal( "Build", profile.Mission );
        Assert.True( profile.HasSeenRoadGuide );
    }

    [Fact]
    public void Map_MissingHabitsReportReminder_MapsToNull()
    {
        Assert.Null( m_mapper.Map<UserReminderDto>( (UserReminder?)null ) );
    }
}
