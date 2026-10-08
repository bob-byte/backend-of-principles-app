using System.Threading;

namespace BusinessLogic;

public class AiAssistantService : IAiAssistantService
{
    private const int MaxPromptChars = 8000;
    private const int MaxChatActiveHabits = 8;
    private const int MaxChatOpenTasks = 8;

    private readonly AppDbContext m_dbContext;
    private readonly IAiService m_aiService;

    public AiAssistantService( AppDbContext dbContext, IAiService aiService )
    {
        m_dbContext = dbContext;
        m_aiService = aiService;
    }

    public async Task<ServiceResult<AiChatSession>> PrepareChatAsync( User user, AiChatRequest request )
    {
        List<AiChatMessage> messages = ToChatMessages( request );
        if (messages.Count == 0)
        {
            return AiError( 400, "PromptIsRequired" );
        }

        ChatUserContext userContext = await BuildChatUserContextAsync( user ).DefaultConfigureAwait();
        return new AiChatSession( messages, userContext );
    }

    public IAsyncEnumerable<string> StreamChatAsync( AiChatSession session, CancellationToken cancellationToken )
    {
        return m_aiService.StreamChatAsync( session.Messages, session.UserContext, cancellationToken );
    }

    public async Task<ServiceResult<AiTaskDraftDto>> ParseTaskAsync( AiParseTaskRequest request, CancellationToken cancellationToken )
    {
        string prompt = request?.Prompt?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace( prompt ))
        {
            return AiError( 400, "PromptIsRequired" );
        }

        if (prompt.Length > MaxPromptChars)
        {
            prompt = prompt[..MaxPromptChars];
        }

        try
        {
            AiTaskDraftResult draft = await m_aiService
                .ParseTaskDraftAsync(
                    prompt,
                    request?.LocalDate,
                    request?.UtcOffsetMinutes,
                    cancellationToken )
                .DefaultConfigureAwait();

            return new AiTaskDraftDto
            {
                Title = draft.Title,
                Description = draft.Description,
                Priority = draft.Priority,
                Theme = draft.Theme,
                DueDate = draft.DueDate,
                AllDay = draft.AllDay,
                Reminders = draft.Reminders?.ToList() ?? new List<int>(),
                Subtasks = draft.Subtasks?.ToList() ?? new List<string>()
            };
        }
        catch (AiServiceException ex)
        {
            return AiError( ex.StatusCode, ex.Message );
        }
    }

    public async Task<ServiceResult<AiRecommendHabitsResponse>> RecommendHabitsAsync(
        User user,
        AiRecommendHabitsRequest request,
        CancellationToken cancellationToken )
    {
        try
        {
            RecommendHabitsContext context = await ToRecommendContextAsync( request, user )
                .DefaultConfigureAwait();
            IReadOnlyList<RecommendedHabitResult> habits = await m_aiService
                .RecommendHabitsAsync( context, cancellationToken )
                .DefaultConfigureAwait();

            return new AiRecommendHabitsResponse
            {
                Habits = habits.Select( h => new AiRecommendedHabitDto
                {
                    Name = h.Name,
                    ReasonToFollow = h.ReasonToFollow
                } ).ToList()
            };
        }
        catch (AiServiceException ex)
        {
            return AiError( ex.StatusCode, ex.Message );
        }
    }

    public async Task<ServiceResult<AiTitleResponse>> GenerateTitleAsync( AiTitleRequest request, CancellationToken cancellationToken )
    {
        string userMessage = request?.UserMessage?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace( userMessage ))
        {
            return AiError( 400, "PromptIsRequired" );
        }

        try
        {
            string title = await m_aiService
                .GenerateConversationTitleAsync(
                    userMessage,
                    request?.AssistantMessage,
                    cancellationToken )
                .DefaultConfigureAwait();

            return new AiTitleResponse { Title = title };
        }
        catch (AiServiceException ex)
        {
            return AiError( ex.StatusCode, ex.Message );
        }
    }

    public async Task<ServiceResult<AiSuggestProfileTextResponse>> SuggestProfileTextAsync(
        User user,
        AiSuggestProfileTextRequest request,
        CancellationToken cancellationToken )
    {
        try
        {
            SuggestProfileTextContext context = await ToSuggestProfileTextContextAsync( request, user )
                .DefaultConfigureAwait();
            IReadOnlyList<ProfileTextSuggestionResult> suggestions = await m_aiService
                .SuggestProfileTextAsync( context, cancellationToken )
                .DefaultConfigureAwait();

            return new AiSuggestProfileTextResponse
            {
                Suggestions = suggestions.Select( s => new AiProfileTextSuggestionDto
                {
                    Text = s.Text,
                    Reason = s.Reason
                } ).ToList()
            };
        }
        catch (AiServiceException ex)
        {
            return AiError( ex.StatusCode, ex.Message );
        }
    }

    public async Task<ServiceResult<AiRecommendGoalsResponse>> RecommendGoalsAsync(
        User user,
        AiRecommendGoalsRequest request,
        CancellationToken cancellationToken )
    {
        try
        {
            RecommendGoalsContext context = await ToRecommendGoalsContextAsync( request, user )
                .DefaultConfigureAwait();

            IReadOnlyList<RecommendedGoalResult> goals = await m_aiService
                .RecommendGoalsAsync( context, cancellationToken )
                .DefaultConfigureAwait();

            return new AiRecommendGoalsResponse
            {
                Goals = goals.Select( g => new AiRecommendedGoalDto
                {
                    Name = g.Name,
                    Reason = g.Reason
                } ).ToList()
            };
        }
        catch (AiServiceException ex)
        {
            return AiError( ex.StatusCode, ex.Message );
        }
    }

    private static ServiceError AiError( int statusCode, string message )
    {
        return new ServiceError( statusCode, new { error = message } );
    }

    private async Task<ChatUserContext> BuildChatUserContextAsync( User user )
    {
        List<string> habits = await LoadActiveHabitNamesAsync( user.Id, MaxChatActiveHabits )
            .DefaultConfigureAwait();
        List<string> goals = await LoadGoalNamesAsync( user.Id ).DefaultConfigureAwait();
        List<string> tasks = await LoadOpenTaskNamesAsync( user.Id ).DefaultConfigureAwait();

        return new ChatUserContext
        {
            Name = NullIfEmpty( user.Name ),
            Gender = FormatChatGender( user.Gender ),
            Mission = NullIfEmpty( user.Mission ),
            MainSlogan = NullIfEmpty( user.MainSlogan ),
            Habits = habits,
            Goals = goals,
            Tasks = tasks
        };
    }

    private async Task<RecommendHabitsContext> ToRecommendContextAsync(
        AiRecommendHabitsRequest request,
        User user )
    {
        string culture = ParseCulture( request?.Culture );

        string? goal = NullIfEmpty( request?.Goal );
        List<string> goals = SanitizeNames( request?.Goals );
        List<string> currentHabits = SanitizeNames( request?.CurrentHabits );
        List<string> areasOfLife = SanitizeNames( request?.AreasOfLife );

        if (currentHabits.Count == 0)
        {
            currentHabits = await LoadActiveHabitNamesAsync( user.Id ).DefaultConfigureAwait();
        }

        if (string.IsNullOrWhiteSpace( goal ) && goals.Count == 0)
        {
            goals = await LoadGoalNamesAsync( user.Id ).DefaultConfigureAwait();
        }

        return new RecommendHabitsContext
        {
            Culture = culture,
            Goal = goal,
            Goals = goals,
            CurrentHabits = currentHabits,
            AreasOfLife = areasOfLife,
            Mission = NullIfEmpty( request?.Mission ) ?? NullIfEmpty( user.Mission ),
            MainSlogan = NullIfEmpty( request?.MainSlogan ) ?? NullIfEmpty( user.MainSlogan ),
            Gender = FormatRecommendGender( ResolveGender( request?.Gender, user ) )
        };
    }

    private async Task<SuggestProfileTextContext> ToSuggestProfileTextContextAsync(
        AiSuggestProfileTextRequest request,
        User user )
    {
        List<string> goals = SanitizeNames( request?.Goals );
        if (goals.Count == 0)
        {
            goals = await LoadGoalNamesAsync( user.Id ).DefaultConfigureAwait();
        }

        return new SuggestProfileTextContext
        {
            Kind = ParseProfileTextKind( request?.Kind ),
            Culture = ParseCulture( request?.Culture ),
            Name = NullIfEmpty( user.Name ),
            Gender = FormatRecommendGender( ResolveGender( request?.Gender, user ) ),
            Mission = NullIfEmpty( request?.Mission ) ?? NullIfEmpty( user.Mission ),
            MainSlogan = NullIfEmpty( request?.MainSlogan ) ?? NullIfEmpty( user.MainSlogan ),
            Draft = NullIfEmpty( request?.Draft ),
            Hint = NullIfEmpty( request?.Hint ),
            Goals = goals
        };
    }

    private async Task<RecommendGoalsContext> ToRecommendGoalsContextAsync(
        AiRecommendGoalsRequest request,
        User user )
    {
        List<string> existingGoals = SanitizeNames( request?.ExistingGoals );
        if (existingGoals.Count == 0)
        {
            existingGoals = await LoadGoalNamesAsync( user.Id ).DefaultConfigureAwait();
        }

        return new RecommendGoalsContext
        {
            Culture = ParseCulture( request?.Culture ),
            AreaOfLife = NullIfEmpty( request?.AreaOfLife ),
            ExistingGoals = existingGoals,
            Draft = NullIfEmpty( request?.Draft ),
            Mission = NullIfEmpty( request?.Mission ) ?? NullIfEmpty( user.Mission ),
            MainSlogan = NullIfEmpty( request?.MainSlogan ) ?? NullIfEmpty( user.MainSlogan ),
            Gender = FormatRecommendGender( ResolveGender( request?.Gender, user ) )
        };
    }

    private static string ParseCulture( string? culture )
    {
        return string.IsNullOrWhiteSpace( culture ) ? "uk" : culture.Trim();
    }

    private static Gender ResolveGender( int? requested, User user )
    {
        return requested is int value && Enum.IsDefined( typeof( Gender ), value )
            ? (Gender)value
            : user.Gender;
    }

    private static ProfileTextKind ParseProfileTextKind( string? raw )
    {
        string value = ( raw ?? string.Empty ).Trim().ToLowerInvariant();
        if (value is "mission" or "missions")
        {
            return ProfileTextKind.Mission;
        }

        return ProfileTextKind.Slogan;
    }

    private async Task<List<string>> LoadActiveHabitNamesAsync( long userId, int? maxCount = null )
    {
        IQueryable<string> query = m_dbContext.UserHabits
            .Where( h => h.UserId == userId && !h.IsArchived && h.Status == StatusOfHabit.InProgress )
            .OrderBy( h => h.Priority )
            .Select( h => h.Name );

        if (maxCount is int limit)
        {
            query = query.Take( limit );
        }

        List<string> names = await query
            .ToListAsync()
            .DefaultConfigureAwait();

        return SanitizeNames( names );
    }

    private async Task<List<string>> LoadGoalNamesAsync( long userId )
    {
        List<string> names = await m_dbContext.UserGoals
            .Where( g => g.UserId == userId )
            .OrderBy( g => g.Id )
            .Select( g => g.Name )
            .ToListAsync()
            .DefaultConfigureAwait();

        return SanitizeNames( names );
    }

    private async Task<List<string>> LoadOpenTaskNamesAsync( long userId )
    {
        // Keep the helper prompt small: only the nearest scheduled / recently updated open tasks.
        List<string> names = await m_dbContext.Tasks
            .Where( t => t.UserId == userId && !t.IsCompleted )
            .OrderBy( t => t.Date == null )
            .ThenBy( t => t.Date )
            .ThenBy( t => t.Time == null )
            .ThenBy( t => t.Time )
            .ThenByDescending( t => t.UpdatedAt )
            .Select( t => t.Name )
            .Take( MaxChatOpenTasks )
            .ToListAsync()
            .DefaultConfigureAwait();

        return SanitizeNames( names );
    }

    private static string FormatChatGender( Gender gender )
    {
        return gender switch
        {
            Gender.Man => "man",
            Gender.Woman => "woman",
            _ => "othersex"
        };
    }

    private static string FormatRecommendGender( Gender gender )
    {
        return gender switch
        {
            Gender.Man => "man",
            Gender.Woman => "woman",
            _ => "other"
        };
    }

    private static List<string> SanitizeNames( IEnumerable<string>? values )
    {
        if (values is null)
        {
            return new List<string>();
        }

        return values
            .Where( v => !string.IsNullOrWhiteSpace( v ) )
            .Select( v => v.Trim() )
            .Where( v => v.Length > 0 )
            .Take( 50 )
            .Select( v => v.Length > 255 ? v[..255] : v )
            .ToList();
    }

    private static string? NullIfEmpty( string? value )
    {
        if (string.IsNullOrWhiteSpace( value ))
        {
            return null;
        }

        string trimmed = value.Trim();
        return trimmed.Length > MaxPromptChars ? trimmed[..MaxPromptChars] : trimmed;
    }

    private static List<AiChatMessage> ToChatMessages( AiChatRequest request )
    {
        List<AiChatMessage> messages = new();
        if (request?.Messages != null)
        {
            foreach (AiChatMessageDto dto in request.Messages)
            {
                if (dto is null)
                {
                    continue;
                }

                IReadOnlyList<AiChatAttachment> attachments = ParseAttachments( dto.Attachments );
                if (string.IsNullOrWhiteSpace( dto.Content ) && attachments.Count == 0)
                {
                    continue;
                }

                messages.Add( new AiChatMessage(
                    dto.Role ?? "user",
                    dto.Content ?? string.Empty,
                    attachments ) );
            }
        }

        string prompt = request?.Prompt?.Trim() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace( prompt ))
        {
            messages.Add( new AiChatMessage( "user", prompt ) );
        }

        return messages;
    }

    private static IReadOnlyList<AiChatAttachment> ParseAttachments(
        IReadOnlyList<AiChatAttachmentDto>? dtos )
    {
        if (dtos is null || dtos.Count == 0)
        {
            return Array.Empty<AiChatAttachment>();
        }

        List<AiChatAttachment> attachments = new();
        foreach (AiChatAttachmentDto dto in dtos)
        {
            if (dto is null)
            {
                continue;
            }

            byte[] data = Array.Empty<byte>();
            if (!string.IsNullOrWhiteSpace( dto.Data ))
            {
                try
                {
                    data = Convert.FromBase64String( dto.Data.Trim() );
                }
                catch (FormatException)
                {
                    continue;
                }
            }

            attachments.Add( new AiChatAttachment(
                dto.FileName?.Trim() ?? string.Empty,
                dto.MimeType?.Trim() ?? string.Empty,
                data ) );
        }

        return attachments;
    }
}
