using BusinessLogic;
using BusinessLogic.Models;
using Moq;
using SET.DataAccess;
using SET.Shared.Models;
using SET.UnitTests.TestSupport;

namespace SET.UnitTests.Services;

public class AiAssistantServiceTests
{
    private readonly AppDbContext m_db = TestDb.Create();
    private readonly Mock<IAiService> m_ai = new( MockBehavior.Strict );

    private AiAssistantService CreateSut() => new( m_db, m_ai.Object );

    private Task<User> AddUserAsync( Gender gender = Gender.Woman ) =>
        TestData.AddUserAsync( m_db, 1, configure: u =>
        {
            u.Name = " Olena ";
            u.Gender = gender;
            u.Mission = "Mission";
            u.MainSlogan = "  ";
        } );

    [Fact]
    public async Task PrepareChatAsync_requires_some_content()
    {
        User user = await AddUserAsync();

        ServiceResult<AiChatSession> result = await CreateSut().PrepareChatAsync( user, new AiChatRequest
        {
            Prompt = "  ",
            Messages = new List<AiChatMessageDto> { new() { Role = "user", Content = " " } },
        } );

        Assert.Equal( 400, result.Error!.StatusCode );
        Assert.Equal( "PromptIsRequired", TestData.AiErrorMessage( result.Error ) );
    }

    [Fact]
    public async Task PrepareChatAsync_appends_prompt_and_defaults_role()
    {
        User user = await AddUserAsync();

        ServiceResult<AiChatSession> result = await CreateSut().PrepareChatAsync( user, new AiChatRequest
        {
            Messages = new List<AiChatMessageDto>
            {
                new() { Role = null!, Content = "earlier" },
                new() { Role = "assistant", Content = "reply" },
                null!,
            },
            Prompt = " now ",
        } );

        Assert.Equal(
            new[] { ("user", "earlier"), ("assistant", "reply"), ("user", "now") },
            result.Value!.Messages.Select( m => (m.Role, m.Content) ) );
    }

    [Fact]
    public async Task PrepareChatAsync_builds_capped_user_context()
    {
        User user = await AddUserAsync( Gender.Other );
        for (int i = 0; i < 10; i++)
        {
            int priority = i;
            await TestData.AddHabitAsync( m_db, 1, $"Habit {i}", h => h.Priority = priority );
            await TestData.AddTaskAsync( m_db, 1, $"Task {i}" );
        }

        await TestData.AddHabitAsync( m_db, 1, "Archived", h => { h.IsArchived = true; h.Priority = -1; } );
        await TestData.AddTaskAsync( m_db, 1, "Done", t => t.IsCompleted = true );
        await TestData.AddGoalAsync( m_db, 1, " Goal " );
        await TestData.AddHabitAsync( m_db, 2, "Foreign" );

        ChatUserContext context = (await CreateSut().PrepareChatAsync( user, new AiChatRequest { Prompt = "hi" } ))
            .Value!.UserContext;

        Assert.Equal( "Olena", context.Name );
        Assert.Equal( "othersex", context.Gender );
        Assert.Equal( "Mission", context.Mission );
        Assert.Null( context.MainSlogan );
        Assert.Equal( Enumerable.Range( 0, 8 ).Select( i => $"Habit {i}" ), context.Habits );
        Assert.Equal( 8, context.Tasks.Count );
        Assert.DoesNotContain( "Done", context.Tasks );
        Assert.Equal( new[] { "Goal" }, context.Goals );
    }

    [Fact]
    public async Task StreamChatAsync_forwards_session_to_ai_service()
    {
        AiChatSession session = new( new[] { new AiChatMessage( "user", "hi" ) }, new ChatUserContext() );
        m_ai.Setup( a => a.StreamChatAsync( session.Messages, session.UserContext, It.IsAny<CancellationToken>() ) )
            .Returns( Chunks( "a", "b" ) );

        List<string> chunks = new();
        await foreach (string chunk in CreateSut().StreamChatAsync( session, CancellationToken.None ))
        {
            chunks.Add( chunk );
        }

        Assert.Equal( new[] { "a", "b" }, chunks );
    }

    [Fact]
    public async Task ParseTaskAsync_requires_prompt()
    {
        ServiceResult<AiTaskDraftDto> result = await CreateSut().ParseTaskAsync( new AiParseTaskRequest { Prompt = " " }, default );

        Assert.Equal( "PromptIsRequired", TestData.AiErrorMessage( result.Error! ) );
    }

    [Fact]
    public async Task ParseTaskAsync_caps_prompt_and_maps_draft()
    {
        string? sentPrompt = null;
        m_ai.Setup( a => a.ParseTaskDraftAsync( It.IsAny<string>(), "2026-10-02", 180, It.IsAny<CancellationToken>() ) )
            .Callback<string, string?, int?, CancellationToken>( ( p, _, _, _ ) => sentPrompt = p )
            .ReturnsAsync( new AiTaskDraftResult
            {
                Title = "Call mom",
                Description = "Sunday",
                DueDate = "2026-10-04",
                AllDay = true,
                Reminders = new[] { 15 },
                Subtasks = new[] { "Buy flowers" },
            } );

        ServiceResult<AiTaskDraftDto> result = await CreateSut().ParseTaskAsync( new AiParseTaskRequest
        {
            Prompt = new string( 'p', 9000 ),
            LocalDate = "2026-10-02",
            UtcOffsetMinutes = 180,
        }, default );

        Assert.Equal( 8000, sentPrompt!.Length );
        Assert.Equal( "Call mom", result.Value!.Title );
        Assert.Equal( new[] { 15 }, result.Value.Reminders );
        Assert.Equal( new[] { "Buy flowers" }, result.Value.Subtasks );
    }

    [Fact]
    public async Task ParseTaskAsync_maps_ai_failures_to_status_and_error_body()
    {
        m_ai.Setup( a => a.ParseTaskDraftAsync( It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>() ) )
            .ThrowsAsync( new AiServiceException( "AiIsDown", 503 ) );

        ServiceResult<AiTaskDraftDto> result = await CreateSut().ParseTaskAsync( new AiParseTaskRequest { Prompt = "x" }, default );

        Assert.Equal( 503, result.Error!.StatusCode );
        Assert.Equal( "AiIsDown", TestData.AiErrorMessage( result.Error ) );
    }

    [Fact]
    public async Task RecommendHabitsAsync_falls_back_to_stored_habits_goals_and_profile()
    {
        User user = await AddUserAsync( Gender.Man );
        await TestData.AddHabitAsync( m_db, 1, "Read" );
        await TestData.AddGoalAsync( m_db, 1, "Learn" );
        RecommendHabitsContext? context = null;
        m_ai.Setup( a => a.RecommendHabitsAsync( It.IsAny<RecommendHabitsContext>(), It.IsAny<CancellationToken>() ) )
            .Callback<RecommendHabitsContext, CancellationToken>( ( c, _ ) => context = c )
            .ReturnsAsync( new[] { new RecommendedHabitResult { Name = "Walk", ReasonToFollow = "Health" } } );

        ServiceResult<AiRecommendHabitsResponse> result =
            await CreateSut().RecommendHabitsAsync( user, new AiRecommendHabitsRequest(), default );

        Assert.Equal( "Walk", Assert.Single( result.Value!.Habits ).Name );
        Assert.Equal( "uk", context!.Culture );
        Assert.Equal( new[] { "Read" }, context.CurrentHabits );
        Assert.Equal( new[] { "Learn" }, context.Goals );
        Assert.Equal( "Mission", context.Mission );
        Assert.Equal( "man", context.Gender );
    }

    [Fact]
    public async Task RecommendHabitsAsync_prefers_request_values()
    {
        User user = await AddUserAsync();
        await TestData.AddGoalAsync( m_db, 1, "Stored goal" );
        RecommendHabitsContext? context = null;
        m_ai.Setup( a => a.RecommendHabitsAsync( It.IsAny<RecommendHabitsContext>(), It.IsAny<CancellationToken>() ) )
            .Callback<RecommendHabitsContext, CancellationToken>( ( c, _ ) => context = c )
            .ReturnsAsync( Array.Empty<RecommendedHabitResult>() );

        await CreateSut().RecommendHabitsAsync( user, new AiRecommendHabitsRequest
        {
            Culture = " en ",
            Goal = "Run",
            CurrentHabits = new List<string> { " Stretch ", "", new string( 'h', 300 ) },
            Mission = "Request mission",
            Gender = (int)Gender.Other,
        }, default );

        Assert.Equal( "en", context!.Culture );
        Assert.Equal( "Run", context.Goal );
        Assert.Empty( context.Goals );
        Assert.Equal( new[] { "Stretch", new string( 'h', 255 ) }, context.CurrentHabits );
        Assert.Equal( "Request mission", context.Mission );
        Assert.Equal( "other", context.Gender );
    }

    [Fact]
    public async Task RecommendHabitsAsync_ignores_undefined_gender_from_request()
    {
        User user = await AddUserAsync( Gender.Woman );
        RecommendHabitsContext? context = null;
        m_ai.Setup( a => a.RecommendHabitsAsync( It.IsAny<RecommendHabitsContext>(), It.IsAny<CancellationToken>() ) )
            .Callback<RecommendHabitsContext, CancellationToken>( ( c, _ ) => context = c )
            .ReturnsAsync( Array.Empty<RecommendedHabitResult>() );

        await CreateSut().RecommendHabitsAsync( user, new AiRecommendHabitsRequest { Gender = 42 }, default );

        Assert.Equal( "woman", context!.Gender );
    }

    [Fact]
    public async Task GenerateTitleAsync_requires_user_message_and_returns_title()
    {
        AiAssistantService service = CreateSut();
        Assert.Equal( "PromptIsRequired", TestData.AiErrorMessage( (await service.GenerateTitleAsync( new AiTitleRequest(), default )).Error! ) );

        m_ai.Setup( a => a.GenerateConversationTitleAsync( "hello", "hi", It.IsAny<CancellationToken>() ) )
            .ReturnsAsync( "Greeting" );

        ServiceResult<AiTitleResponse> result =
            await service.GenerateTitleAsync( new AiTitleRequest { UserMessage = " hello ", AssistantMessage = "hi" }, default );

        Assert.Equal( "Greeting", result.Value!.Title );
    }

    [Theory]
    [InlineData( "mission", ProfileTextKind.Mission )]
    [InlineData( " Missions ", ProfileTextKind.Mission )]
    [InlineData( "slogan", ProfileTextKind.Slogan )]
    [InlineData( null, ProfileTextKind.Slogan )]
    public async Task SuggestProfileTextAsync_parses_kind( string? kind, ProfileTextKind expected )
    {
        User user = await AddUserAsync();
        SuggestProfileTextContext? context = null;
        m_ai.Setup( a => a.SuggestProfileTextAsync( It.IsAny<SuggestProfileTextContext>(), It.IsAny<CancellationToken>() ) )
            .Callback<SuggestProfileTextContext, CancellationToken>( ( c, _ ) => context = c )
            .ReturnsAsync( new[] { new ProfileTextSuggestionResult { Text = "Be kind", Reason = "Why" } } );

        ServiceResult<AiSuggestProfileTextResponse> result =
            await CreateSut().SuggestProfileTextAsync( user, new AiSuggestProfileTextRequest { Kind = kind! }, default );

        Assert.Equal( expected, context!.Kind );
        Assert.Equal( "Olena", context.Name );
        Assert.Equal( "Be kind", Assert.Single( result.Value!.Suggestions ).Text );
    }

    [Fact]
    public async Task RecommendGoalsAsync_requires_area_of_life_without_calling_ai()
    {
        User user = await AddUserAsync();

        ServiceResult<AiRecommendGoalsResponse> result =
            await CreateSut().RecommendGoalsAsync( user, new AiRecommendGoalsRequest { AreaOfLife = " " }, default );

        Assert.Equal( 400, result.Error!.StatusCode );
        Assert.Equal( "AreaOfLifeIsRequired", TestData.AiErrorMessage( result.Error ) );
    }

    [Fact]
    public async Task RecommendGoalsAsync_uses_stored_goals_when_request_has_none()
    {
        User user = await AddUserAsync();
        await TestData.AddGoalAsync( m_db, 1, "Existing" );
        RecommendGoalsContext? context = null;
        m_ai.Setup( a => a.RecommendGoalsAsync( It.IsAny<RecommendGoalsContext>(), It.IsAny<CancellationToken>() ) )
            .Callback<RecommendGoalsContext, CancellationToken>( ( c, _ ) => context = c )
            .ReturnsAsync( new[] { new RecommendedGoalResult { Name = "New", Reason = "Because" } } );

        ServiceResult<AiRecommendGoalsResponse> result =
            await CreateSut().RecommendGoalsAsync( user, new AiRecommendGoalsRequest { AreaOfLife = "Health" }, default );

        Assert.Equal( "New", Assert.Single( result.Value!.Goals ).Name );
        Assert.Equal( "Health", context!.AreaOfLife );
        Assert.Equal( new[] { "Existing" }, context.ExistingGoals );
    }

    private static async IAsyncEnumerable<string> Chunks( params string[] chunks )
    {
        foreach (string chunk in chunks)
        {
            await Task.Yield();
            yield return chunk;
        }
    }
}
