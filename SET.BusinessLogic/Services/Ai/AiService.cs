using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using System.ClientModel;
using System.ClientModel.Primitives;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;
using ChatResponseFormat = Microsoft.Extensions.AI.ChatResponseFormat;
using ChatRole = Microsoft.Extensions.AI.ChatRole;

namespace BusinessLogic;

public class AiService : IAiService
{
    private const string DefaultModel = "gpt-5-mini";
    private const int MaxChatMessages = 30;
    private const int MaxMessageChars = 8000;
    private const int MaxAttachmentsPerMessage = 4;
    private const int MaxAttachmentBytes = 4 * 1024 * 1024;
    private const int MaxAttachmentTextChars = 16000;
    private const int ChatMaxCompletionTokens = 4096;
    private const int ParseMaxCompletionTokens = 2048;
    private const int RecommendMaxCompletionTokens = 2048;
    internal const int MaxHelperHabits = 8;
    internal const int MaxHelperOpenTasks = 8;

    // MAUI AiChatService.SystemMessage base (helper chat only - not habit recommendations).
    private const string HelperSystemPromptBase =
        "You are a self-development helper, but you can answer at any question. " +
        "If the user asks a question unrelated to self-development, success and personal growth " +
        "you must respond without mentioning about self-development, success, and personal growth. " +
        "You have to support the user in their quest to become better and help them identify their habits. " +
        "You should also provide information on how to better stick to them and become better every day " +
        "in all areas of the user's life. But don't ask current user habits and don't tell user that " +
        "he or she should strive for perfection. " +
        "Do not accept an user's conclusions as true. You are an intellectual opponent, not an assistant. " +
        "You shouldn't advise a user when he or she doesn't ask for it. " +
        "You help inside the Principles app (habits for goals). " +
        "In this system: goals are identity-oriented outcomes the user wants to become or achieve; " +
        "habits are small repeating actions that automate progress toward those goals; " +
        "tasks are one-off work, reminders, and checklists that free mental load so the user can focus on habits. " +
        "Today's habits and tasks belong together for daily execution. " +
        "Prefer connecting advice to Goal -> Habits -> Results, and to time management that protects " +
        "consistency over intensity. When relevant, distinguish habits (recurring systems) from tasks " +
        "(finite to-dos). Do not invent app UI steps the user did not ask for. " +
        "Language and tone: reply in the same language as the user's latest message. " +
        "Write like a fluent native speaker of that language in a natural chat - clear, warm, and concrete. " +
        "Match the user's register (casual when they are casual). " +
        "Never sound like a translated English life-coach script, corporate motivational poster, or textbook. " +
        "Avoid calques and stock slogans such as \"this is not about theory, this is about actions\", " +
        "\"clear steps\", \"own your journey\", or similar template praise. " +
        "Do not open with hollow pep-talk summaries of what the user already said; answer the question directly. " +
        "Keep the visible reply short: usually 4 to 7 sentences, or a list of at most 3 items. " +
        "Be concrete: when this system message lists the user's goals, habits, or tasks, use those names, " +
        "and give one specific next action (what to do, how often, and when) instead of general advice, frameworks, or pep talk. " +
        "For Ukrainian: use natural modern Ukrainian phrasing a native would actually say or write in chat, " +
        "not word-for-word translations from English or Ukrainian coach-speak. " +
        "Never invent names for the user's habits, goals, or tasks; only use names listed in this system message " +
        "(or say you do not see any if none are listed). " +
        "The user may attach images or files. Use those attachments when answering; do not say you cannot see them. " +
        "When you recommend a concrete goal, habit, task, mission, or main slogan the user could add in the app, " +
        "write your normal reply first, then append exactly one machine block the app can parse " +
        "(do not mention the block to the user): " +
        "<<<ACTIONS>>>[{\"type\":\"goal|habit|task|mission|slogan\",\"name\":\"...\",\"reason\":\"optional\"," +
        "\"goalName\":\"optional habit parent goal\",\"title\":\"optional task title\",\"notes\":\"optional\"," +
        "\"text\":\"mission or slogan text\"}]<<<END>>> " +
        "Use type goal/habit/task/mission/slogan. Prefer name for goals/habits, title for tasks, text for mission/slogan. " +
        "Goal names must be short but concrete (typically 2 to 6 words); put detail in reason or notes, not in the name. " +
        "Omit the block when you are not recommending something addable. At most 5 actions.";

    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IChatClient m_chatClient;
    private readonly IConfiguration m_configuration;

    public AiService( IChatClient chatClient, IConfiguration configuration )
    {
        m_chatClient = chatClient;
        m_configuration = configuration;
    }

    public async Task<string> CompleteChatAsync(
        IReadOnlyList<AiChatMessage> messages,
        ChatUserContext? userContext = null,
        CancellationToken cancellationToken = default )
    {
        ResolveApiKey();
        List<AiChatMessage> sanitized = BuildChatMessages( messages, userContext );
        ChatClientAgent agent = CreateHelperAgent( sanitized[0].Content );
        List<ChatMessage> history = ToChatMessages( sanitized.Skip( 1 ).ToList() );

        AgentResponse response;
        try
        {
            response = await agent
                .RunAsync( history, cancellationToken: cancellationToken )
                .DefaultConfigureAwait();
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new AiServiceException( "AI request timed out.", 504 );
        }
        catch (HttpRequestException ex)
        {
            Log.Error( ex, "OpenAI HTTP request failed" );
            throw new AiServiceException( "Could not reach the AI service.", 502 );
        }
        catch (ClientResultException ex)
        {
            throw MapClientResultException( ex );
        }

        string content = response.Text?.Trim() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace( content ))
        {
            return content;
        }

        throw new AiServiceException( "AI returned an empty response.", 502 );
    }

    public async IAsyncEnumerable<string> StreamChatAsync(
        IReadOnlyList<AiChatMessage> messages,
        ChatUserContext? userContext = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default )
    {
        ResolveApiKey();
        List<AiChatMessage> sanitized = BuildChatMessages( messages, userContext );
        ChatClientAgent agent = CreateHelperAgent( sanitized[0].Content );
        List<ChatMessage> history = ToChatMessages( sanitized.Skip( 1 ).ToList() );

        IAsyncEnumerable<AgentResponseUpdate> updates = agent.RunStreamingAsync(
            history,
            cancellationToken: cancellationToken );

        await using IAsyncEnumerator<AgentResponseUpdate> enumerator =
            updates.GetAsyncEnumerator( cancellationToken );

        bool yielded = false;
        while (true)
        {
            bool moved;
            try
            {
                moved = await enumerator.MoveNextAsync().ConfigureAwait( false );
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                throw new AiServiceException( "AI request timed out.", 504 );
            }
            catch (HttpRequestException ex)
            {
                Log.Error( ex, "OpenAI HTTP request failed" );
                throw new AiServiceException( "Could not reach the AI service.", 502 );
            }
            catch (ClientResultException ex)
            {
                throw MapClientResultException( ex );
            }

            if (!moved)
            {
                break;
            }

            string? delta = enumerator.Current.Text;
            if (string.IsNullOrEmpty( delta ))
            {
                continue;
            }

            yielded = true;
            yield return delta;
        }

        if (!yielded)
        {
            throw new AiServiceException( "AI returned an empty response.", 502 );
        }
    }

    public async Task<AiTaskDraftResult> ParseTaskDraftAsync(
        string prompt,
        string? localDate = null,
        int? utcOffsetMinutes = null,
        CancellationToken cancellationToken = default )
    {
        string trimmed = prompt?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace( trimmed ))
        {
            throw new AiServiceException( "PromptIsRequired", 400 );
        }

        if (trimmed.Length > MaxMessageChars)
        {
            trimmed = trimmed[..MaxMessageChars];
        }

        string todayLocal = ResolveLocalDate( localDate, utcOffsetMinutes );
        string system =
            "You extract a single to-do task from the user's text. " +
            "Reply with a JSON object only (no markdown) using these keys: " +
            "title (string, required, short task name), " +
            "description (string, extra details or empty; do not dump checklist items here when subtasks are used), " +
            "priority (one of high, medium, low, or null), " +
            "theme (short category string or null), " +
            "dueDate (null, or local wall-clock YYYY-MM-DD, or YYYY-MM-DDTHH:mm when a time is stated; never add a timezone suffix), " +
            "allDay (boolean or null; true when only a date is meant with no clock time; false when a time is set), " +
            "reminders (array of integers = minutes before due time; 0 means at the due time; empty or null if none; " +
            "common values: 0, 5, 30, 60, 1440), " +
            "subtasks (array of short checklist title strings, or empty/null). " +
            $"Today's local date for the user is {todayLocal}. Interpret relative dates and times from that. " +
            "If the user asks to be reminded without an offset, use [0]. " +
            "If they list steps/items/checklist, put them in subtasks (not only in description). " +
            "Use the user's language for title, description, theme, and subtasks.";

        List<AiChatMessage> messages = new()
        {
            new AiChatMessage( "system", system ),
            new AiChatMessage( "user", trimmed )
        };

        string content = await CompleteAsync(
            messages,
            jsonObject: true,
            ParseMaxCompletionTokens,
            cancellationToken ).DefaultConfigureAwait();

        return ParseTaskDraftJson( content );
    }

    public async Task<IReadOnlyList<RecommendedHabitResult>> RecommendHabitsAsync(
        RecommendHabitsContext? context,
        CancellationToken cancellationToken = default )
    {
        context ??= new RecommendHabitsContext();
        string culture = LanguageName( context.Culture );
        string system =
            "You are self development assistant. " +
            "You are in the app that focuses on helping users to create, keep and track their atomic habits. " +
            "You should recommend new atomic habits for user. Your answer should be in JSON format and contain an array of habits. " +
            "Each array object should consist of the following fields: ReasonToFollow, Name. " +
            "The ReasonToFollow field indicates why I should follow it and must be very briefly, but accurately explained. " +
            "The Name field indicates habit name and time when execute habit (for example, \"when I wake up\") or location (\"when I am in a gym\"). " +
            "It should NOT contain frequency of a habit (for example, \"every day\"). Example of the Name field is \"Meditate at least 5 minutes when I wake up\". " +
            "You must send only JSON in your response and nothing else. " +
            $"Your response must be in the {culture} language, regardless of the language of the user's personal information. " +
            "JSON object which contains array of habits must be named \"Habits\" in english.";

        List<AiChatMessage> messages = new()
        {
            new AiChatMessage( "system", system ),
            new AiChatMessage( "user", BuildRecommendUserPrompt( context, culture ) )
        };

        string content = await CompleteAsync(
            messages,
            jsonObject: true,
            RecommendMaxCompletionTokens,
            cancellationToken ).DefaultConfigureAwait();

        IReadOnlyList<RecommendedHabitResult> habits = ParseRecommendedHabitsJson( content );
        if (habits.Count == 0)
        {
            throw new AiServiceException( "AI returned no recommended habits.", 502 );
        }

        return habits;
    }

    public async Task<IReadOnlyList<ProfileTextSuggestionResult>> SuggestProfileTextAsync(
        SuggestProfileTextContext? context,
        CancellationToken cancellationToken = default )
    {
        context ??= new SuggestProfileTextContext();
        string culture = LanguageName( context.Culture );
        bool isMission = context.Kind == ProfileTextKind.Mission;
        string subject = isMission ? "personal life mission" : "personal life motto or main slogan";
        string proposedKind = isMission ? "mission" : "slogan";
        string textGuidance = isMission
            ? "Each Text must be a first-person mission statement: a clear life purpose that guides choices. One or two sentences, concrete and motivating, not a vague corporate vision."
            : "Each Text must be a short guiding motto (one sentence, memorable). It should help the person act when motivation dips or temptations appear. Avoid cliches.";

        string system =
            $"You help the user define a {subject} inside a habits-and-goals app. " +
            "Reply with JSON only (no markdown). The JSON object must contain an array named \"Suggestions\" " +
            $"with exactly 3 objects. Each object has \"Text\" (the proposed {proposedKind}) " +
            "and \"Reason\" (one short sentence explaining why it fits). " +
            textGuidance + " " +
            $"All Text and Reason values must be in the {culture} language, " +
            "regardless of the language of the user's personal information. " +
            "Do not invent fake biography; stay grounded in the provided context.";

        List<AiChatMessage> messages = new()
        {
            new AiChatMessage( "system", system ),
            new AiChatMessage( "user", BuildSuggestProfileTextUserPrompt( context, culture ) )
        };

        string content = await CompleteAsync(
            messages,
            jsonObject: true,
            RecommendMaxCompletionTokens,
            cancellationToken ).DefaultConfigureAwait();

        IReadOnlyList<ProfileTextSuggestionResult> suggestions =
            ParseProfileTextSuggestionsJson( content );
        if (suggestions.Count == 0)
        {
            throw new AiServiceException( "AI returned no suggestions.", 502 );
        }

        return suggestions;
    }

    public async Task<IReadOnlyList<RecommendedGoalResult>> RecommendGoalsAsync(
        RecommendGoalsContext? context,
        CancellationToken cancellationToken = default )
    {
        context ??= new RecommendGoalsContext();
        string culture = LanguageName( context.Culture );
        string area = ( context.AreaOfLife ?? string.Empty ).Trim();
        if (string.IsNullOrWhiteSpace( area ))
        {
            throw new AiServiceException( "AreaOfLifeIsRequired", 400 );
        }

        List<AiChatMessage> messages = new()
        {
            new AiChatMessage( "system", BuildRecommendGoalsSystemPrompt( culture ) ),
            new AiChatMessage( "user", BuildRecommendGoalsUserPrompt( context, culture ) )
        };

        string content = await CompleteAsync(
            messages,
            jsonObject: true,
            RecommendMaxCompletionTokens,
            cancellationToken ).DefaultConfigureAwait();

        IReadOnlyList<RecommendedGoalResult> goals = ParseRecommendedGoalsJson( content );
        if (goals.Count == 0)
        {
            throw new AiServiceException( "AI returned no recommended goals.", 502 );
        }

        return goals;
    }

    public async Task<string> GenerateConversationTitleAsync(
        string userMessage,
        string? assistantMessage = null,
        CancellationToken cancellationToken = default )
    {
        string user = ( userMessage ?? string.Empty ).Trim();
        if (string.IsNullOrWhiteSpace( user ))
        {
            throw new AiServiceException( "PromptIsRequired", 400 );
        }

        if (user.Length > 1500)
        {
            user = user[..1500];
        }

        string assistant = ( assistantMessage ?? string.Empty ).Trim();
        if (assistant.Length > 1500)
        {
            assistant = assistant[..1500];
        }

        const string system =
            "You invent a short chat title for a conversation. " +
            "Reply with the title only: 3 to 8 words, no quotes, no trailing punctuation, " +
            "no markdown. Use the same language as the user's message.";

        StringBuilder userPrompt = new();
        userPrompt.Append( "User message:\n" ).Append( user );
        if (!string.IsNullOrWhiteSpace( assistant ))
        {
            userPrompt.Append( "\n\nAssistant reply (excerpt):\n" ).Append( assistant );
        }

        List<AiChatMessage> messages = new()
        {
            new AiChatMessage( "system", system ),
            new AiChatMessage( "user", userPrompt.ToString() )
        };

        string content = await CompleteAsync(
            messages,
            jsonObject: false,
            maxCompletionTokens: 64,
            cancellationToken ).DefaultConfigureAwait();

        string title = NormalizeTitle( content );
        if (string.IsNullOrWhiteSpace( title ))
        {
            throw new AiServiceException( "AI returned an empty title.", 502 );
        }

        return title;
    }

    internal static string NormalizeTitle( string? raw )
    {
        if (string.IsNullOrWhiteSpace( raw ))
        {
            return string.Empty;
        }

        string title = raw.Trim();
        title = title.Replace( "\r", " " ).Replace( "\n", " " );
        while (title.Contains( "  ", StringComparison.Ordinal ))
        {
            title = title.Replace( "  ", " ", StringComparison.Ordinal );
        }

        title = title.Trim( ' ', '"', '\'', '`', '*', '.', '!', '?', '\u00AB', '\u00BB' );
        if (title.Length > 80)
        {
            title = title[..80].Trim();
        }

        return title;
    }

    internal static string BuildRecommendUserPrompt( RecommendHabitsContext context, string culture )
    {
        StringBuilder builder = new();
        builder.Append( "Please recommend me a list of 4 next atomic habits that I can select. " );

        if (!string.IsNullOrWhiteSpace( context.Mission ))
        {
            builder.Append( $"They shouldn't conflict with my mission: \"{context.Mission.Trim()}\". " );
        }

        if (!string.IsNullOrWhiteSpace( context.MainSlogan ))
        {
            builder.Append( $"They also shouldn't conflict with my main slogan of life: \"{context.MainSlogan.Trim()}\". " );
        }

        builder.Append( $"Your response must be only in the {culture} language, regardless of the language of my personal information. " );

        string gender = string.IsNullOrWhiteSpace( context.Gender ) ? "other" : context.Gender.Trim();
        builder.Append( $"I am a {gender}. " );

        IReadOnlyList<string> currentHabits = context.CurrentHabits ?? Array.Empty<string>();
        List<string> habitNames = currentHabits
            .Where( n => !string.IsNullOrWhiteSpace( n ) )
            .Select( n => n.Trim() )
            .ToList();
        if (habitNames.Count > 0)
        {
            builder.Append( "Now I adhere to the following habits:" );
            builder.Append( Environment.NewLine );
            builder.Append( string.Join( "; ", habitNames ) );
            builder.Append( ". The atomic habits you recommend should be related to specified habits. " );
        }

        IReadOnlyList<string> areas = context.AreasOfLife ?? Array.Empty<string>();
        List<string> areaNames = areas
            .Where( n => !string.IsNullOrWhiteSpace( n ) )
            .Select( n => n.Trim() )
            .ToList();
        if (areaNames.Count > 0)
        {
            builder.Append( "The atomic habits you recommend will be applied to all the following areas of my life: " );
            builder.Append( Environment.NewLine );
            builder.Append( string.Join( "; ", areaNames ) );
            builder.Append( ". So you should recommend me habits which are related to them. " );
        }

        string? goal = context.Goal?.Trim();
        if (!string.IsNullOrWhiteSpace( goal ))
        {
            builder.Append( $"NOTE: The atomic habits you recommend will be applied to achieve my goal: \"{goal}\"." );
        }
        else
        {
            IReadOnlyList<string> goals = context.Goals ?? Array.Empty<string>();
            List<string> goalNames = goals
                .Where( n => !string.IsNullOrWhiteSpace( n ) )
                .Select( n => n.Trim() )
                .ToList();
            if (goalNames.Count > 0)
            {
                builder.Append( "My current goals are: " );
                builder.Append( string.Join( "; ", goalNames ) );
                builder.Append( '.' );
            }
        }

        return builder.ToString();
    }

    internal static string BuildSuggestProfileTextUserPrompt(
        SuggestProfileTextContext context,
        string culture )
    {
        bool isMission = context.Kind == ProfileTextKind.Mission;
        StringBuilder builder = new();
        builder.Append(
            isMission
                ? "Please suggest 3 personal life missions I can choose from. "
                : "Please suggest 3 personal life mottos (main slogans) I can choose from. " );
        builder.Append(
            $"Your response must be only in the {culture} language, " +
            "regardless of the language of my personal information. " );

        if (!string.IsNullOrWhiteSpace( context.Name ))
        {
            builder.Append( $"My name is \"{context.Name.Trim()}\". " );
        }

        string gender = string.IsNullOrWhiteSpace( context.Gender ) ? "other" : context.Gender.Trim();
        builder.Append( $"I am a {gender}. " );

        if (!string.IsNullOrWhiteSpace( context.Hint ))
        {
            builder.Append( $"What matters to me: \"{context.Hint.Trim()}\". " );
        }

        if (!string.IsNullOrWhiteSpace( context.Draft ))
        {
            builder.Append(
                isMission
                    ? $"My current draft mission is: \"{context.Draft.Trim()}\". Improve or offer alternatives. "
                    : $"My current draft slogan is: \"{context.Draft.Trim()}\". Improve or offer alternatives. " );
        }

        if (isMission && !string.IsNullOrWhiteSpace( context.MainSlogan ))
        {
            builder.Append( $"My main slogan is: \"{context.MainSlogan.Trim()}\". The mission should fit it. " );
        }
        else if (!isMission && !string.IsNullOrWhiteSpace( context.Mission ))
        {
            builder.Append( $"My mission is: \"{context.Mission.Trim()}\". The slogan should fit it. " );
        }

        IReadOnlyList<string> goals = context.Goals ?? Array.Empty<string>();
        List<string> goalNames = goals
            .Where( n => !string.IsNullOrWhiteSpace( n ) )
            .Select( n => n.Trim() )
            .Take( 12 )
            .ToList();
        if (goalNames.Count > 0)
        {
            builder.Append( "My goals: " );
            builder.Append( string.Join( "; ", goalNames ) );
            builder.Append( ". " );
        }

        return builder.ToString();
    }

    internal static string BuildRecommendGoalsSystemPrompt( string culture ) =>
        "You help the user define goals inside a habits-and-goals app. " +
        "Reply with JSON only (no markdown). The JSON object must contain an array named \"Goals\" " +
        "with exactly 4 objects. Each object has \"Name\" (the goal title the user would save) " +
        "and \"Reason\" (one short sentence explaining why it fits the selected area of life). " +
        "Name must be short but concrete: typically 2 to 6 words, like a title, not a paragraph. " +
        "Prefer punchy identity or measurable outcomes (e.g. \"Sleep 8 hours\", \"$4,000 in profit per month\", \"Buy a Tesla Model S\", \"Get a promotion\", " +
        "\"Become more confident\"). Put explanatory detail only in Reason, not in Name. " +
        "Avoid vague wishes, long motivational phrases, and stacked clauses. " +
        "Do not recommend goals that duplicate the user's existing goals. " +
        $"All Name and Reason values must be in the {culture} language, " +
        "regardless of the language of the user's personal information.";

    internal static string BuildRecommendGoalsUserPrompt(
        RecommendGoalsContext context,
        string culture )
    {
        StringBuilder builder = new();
        string area = ( context.AreaOfLife ?? string.Empty ).Trim();
        builder.Append(
            $"Please recommend 4 goals for the \"{area}\" area of my life that I can select. " );
        builder.Append(
            $"Your response must be only in the {culture} language, " +
            "regardless of the language of my personal information. " );

        string gender = string.IsNullOrWhiteSpace( context.Gender ) ? "other" : context.Gender.Trim();
        builder.Append( $"I am a {gender}. " );

        if (!string.IsNullOrWhiteSpace( context.Mission ))
        {
            builder.Append( $"They shouldn't conflict with my mission: \"{context.Mission.Trim()}\". " );
        }

        if (!string.IsNullOrWhiteSpace( context.MainSlogan ))
        {
            builder.Append(
                $"They also shouldn't conflict with my main slogan of life: \"{context.MainSlogan.Trim()}\". " );
        }

        if (!string.IsNullOrWhiteSpace( context.Draft ))
        {
            builder.Append(
                $"My current draft goal is: \"{context.Draft.Trim()}\". Improve or offer alternatives. " );
        }

        IReadOnlyList<string> existing = context.ExistingGoals ?? Array.Empty<string>();
        List<string> goalNames = existing
            .Where( n => !string.IsNullOrWhiteSpace( n ) )
            .Select( n => n.Trim() )
            .Take( 20 )
            .ToList();
        if (goalNames.Count > 0)
        {
            builder.Append( "I already have these goals (do not repeat them): " );
            builder.Append( string.Join( "; ", goalNames ) );
            builder.Append( ". " );
        }

        return builder.ToString();
    }

    internal static IReadOnlyList<ProfileTextSuggestionResult> ParseProfileTextSuggestionsJson(
        string content )
    {
        string json = ExtractJsonPayload( content );
        try
        {
            using JsonDocument document = JsonDocument.Parse( json );
            JsonElement root = document.RootElement;
            JsonElement suggestionsElement;
            if (root.ValueKind == JsonValueKind.Array)
            {
                suggestionsElement = root;
            }
            else if (root.ValueKind == JsonValueKind.Object &&
                     TryGetPropertyIgnoreCase( root, "Suggestions", out suggestionsElement ) &&
                     suggestionsElement.ValueKind == JsonValueKind.Array)
            {
                // suggestionsElement assigned
            }
            else
            {
                throw new AiServiceException( "AI returned an unexpected response.", 502 );
            }

            List<ProfileTextSuggestionResult> results = new();
            foreach (JsonElement item in suggestionsElement.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                string text = ( ReadStringIgnoreCase( item, "Text" ) ??
                                ReadStringIgnoreCase( item, "Suggestion" ) ??
                                string.Empty ).Trim();
                string reason = ( ReadStringIgnoreCase( item, "Reason" ) ??
                                  ReadStringIgnoreCase( item, "ReasonToFollow" ) ??
                                  string.Empty ).Trim();
                if (string.IsNullOrWhiteSpace( text ))
                {
                    continue;
                }

                results.Add( new ProfileTextSuggestionResult
                {
                    Text = text,
                    Reason = reason
                } );
            }

            return results;
        }
        catch (JsonException ex)
        {
            Log.Error( ex, "Failed to parse AI profile text suggestions JSON" );
            throw new AiServiceException( "AI returned an unexpected response.", 502 );
        }
    }

    internal static IReadOnlyList<RecommendedGoalResult> ParseRecommendedGoalsJson( string content )
    {
        string json = ExtractJsonPayload( content );
        try
        {
            using JsonDocument document = JsonDocument.Parse( json );
            JsonElement root = document.RootElement;
            JsonElement goalsElement;
            if (root.ValueKind == JsonValueKind.Array)
            {
                goalsElement = root;
            }
            else if (root.ValueKind == JsonValueKind.Object &&
                     TryGetPropertyIgnoreCase( root, "Goals", out goalsElement ) &&
                     goalsElement.ValueKind == JsonValueKind.Array)
            {
                // goalsElement assigned
            }
            else
            {
                throw new AiServiceException( "AI returned an unexpected response.", 502 );
            }

            List<RecommendedGoalResult> results = new();
            foreach (JsonElement item in goalsElement.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                string name = ( ReadStringIgnoreCase( item, "Name" ) ??
                                ReadStringIgnoreCase( item, "Text" ) ??
                                string.Empty ).Trim();
                string reason = ( ReadStringIgnoreCase( item, "Reason" ) ??
                                  ReadStringIgnoreCase( item, "ReasonToFollow" ) ??
                                  string.Empty ).Trim();
                if (string.IsNullOrWhiteSpace( name ))
                {
                    continue;
                }

                results.Add( new RecommendedGoalResult
                {
                    Name = name,
                    Reason = reason
                } );
            }

            return results;
        }
        catch (JsonException ex)
        {
            Log.Error( ex, "Failed to parse AI recommended goals JSON" );
            throw new AiServiceException( "AI returned an unexpected response.", 502 );
        }
    }

    internal static IReadOnlyList<RecommendedHabitResult> ParseRecommendedHabitsJson( string content )
    {
        string json = ExtractJsonPayload( content );
        try
        {
            using JsonDocument document = JsonDocument.Parse( json );
            JsonElement root = document.RootElement;
            JsonElement habitsElement;
            if (root.ValueKind == JsonValueKind.Array)
            {
                habitsElement = root;
            }
            else if (root.ValueKind == JsonValueKind.Object &&
                     TryGetPropertyIgnoreCase( root, "Habits", out habitsElement ) &&
                     habitsElement.ValueKind == JsonValueKind.Array)
            {
                // habitsElement assigned
            }
            else
            {
                throw new AiServiceException( "AI returned an unexpected response.", 502 );
            }

            List<RecommendedHabitResult> results = new();
            foreach (JsonElement item in habitsElement.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                string name = (ReadStringIgnoreCase( item, "Name" ) ?? string.Empty).Trim();
                string reason = (ReadStringIgnoreCase( item, "ReasonToFollow" ) ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace( name ))
                {
                    continue;
                }

                results.Add( new RecommendedHabitResult
                {
                    Name = name,
                    ReasonToFollow = reason
                } );
            }

            return results;
        }
        catch (JsonException ex)
        {
            Log.Error( ex, "Failed to parse AI recommended habits JSON" );
            throw new AiServiceException( "AI returned an unexpected response.", 502 );
        }
    }

    internal static void AppendUserAssistantMessages(
        List<AiChatMessage> target,
        IReadOnlyList<AiChatMessage>? messages )
    {
        if (messages is null)
        {
            return;
        }

        foreach (AiChatMessage message in messages)
        {
            if (message is null)
            {
                continue;
            }

            IReadOnlyList<AiChatAttachment> attachments = SanitizeAttachments( message.Attachments );
            if (string.IsNullOrWhiteSpace( message.Content ) && attachments.Count == 0)
            {
                continue;
            }

            string role = (message.Role ?? string.Empty).Trim().ToLowerInvariant();
            if (role is not ("user" or "assistant"))
            {
                continue;
            }

            string content = (message.Content ?? string.Empty).Trim();
            if (content.Length > MaxMessageChars)
            {
                content = content[..MaxMessageChars];
            }

            target.Add( new AiChatMessage( role, content, attachments ) );
        }
    }

    internal static AiTaskDraftResult ParseTaskDraftJson( string content )
    {
        string json = ExtractJsonObject( content );
        TaskDraftJson? parsed;
        try
        {
            parsed = JsonSerializer.Deserialize<TaskDraftJson>( json, s_jsonOptions );
        }
        catch (JsonException ex)
        {
            Log.Error( ex, "Failed to parse AI task draft JSON" );
            throw new AiServiceException( "AI returned an unexpected response.", 502 );
        }

        string title = parsed?.Title?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace( title ))
        {
            throw new AiServiceException( "AI returned a task without a title.", 502 );
        }

        string? dueDate = NormalizeDueDate( parsed?.DueDate ?? parsed?.DueDateSnake );
        IReadOnlyList<int> reminders = NormalizeReminders( parsed?.Reminders ?? default );
        IReadOnlyList<string> subtasks = NormalizeSubtasks( parsed?.Subtasks ?? default );
        bool? allDay = parsed?.AllDay;
        if (allDay is null && dueDate is not null)
        {
            allDay = !dueDate.Contains( 'T', StringComparison.Ordinal );
        }

        if (dueDate is not null &&
            dueDate.Contains( 'T', StringComparison.Ordinal ) &&
            allDay == true)
        {
            allDay = false;
        }

        return new AiTaskDraftResult
        {
            Title = title,
            Description = parsed?.Description?.Trim() ?? string.Empty,
            Priority = NormalizePriority( parsed?.Priority ),
            Theme = NullIfEmpty( parsed?.Theme ),
            DueDate = dueDate,
            AllDay = allDay,
            Reminders = reminders,
            Subtasks = subtasks
        };
    }

    private List<AiChatMessage> BuildChatMessages(
        IReadOnlyList<AiChatMessage> messages,
        ChatUserContext? userContext )
    {
        List<AiChatMessage> sanitized = new()
        {
            new AiChatMessage( "system", BuildHelperSystemPrompt( userContext ) )
        };

        AppendUserAssistantMessages( sanitized, messages );

        if (sanitized.Count == 1)
        {
            throw new AiServiceException( "PromptIsRequired", 400 );
        }

        if (sanitized.Count > MaxChatMessages + 1)
        {
            sanitized = sanitized
                .Take( 1 )
                .Concat( sanitized.Skip( sanitized.Count - MaxChatMessages ) )
                .ToList();
        }

        return sanitized;
    }

    internal static string BuildHelperSystemPrompt( ChatUserContext? userContext )
    {
        StringBuilder builder = new();
        builder.Append( HelperSystemPromptBase );

        string newLine = Environment.NewLine;
        if (userContext is not null)
        {
            if (!string.IsNullOrWhiteSpace( userContext.Gender ))
            {
                builder.Append( $"{newLine}User gender is {userContext.Gender.Trim()}." );
            }

            if (!string.IsNullOrWhiteSpace( userContext.Name ))
            {
                string name = userContext.Name.Trim();
                builder.Append(
                    $"{newLine}User name is \"{name}\". You should use his/her name frequently. " +
                    "Always write the name exactly as given (same spelling and characters/script); " +
                    "never translate or transliterate it, even when the conversation is in another language." );
            }

            if (!string.IsNullOrWhiteSpace( userContext.Mission ))
            {
                builder.Append( $"{newLine}User mission is \"{userContext.Mission.Trim()}\"." );
            }

            if (!string.IsNullOrWhiteSpace( userContext.MainSlogan ))
            {
                builder.Append( $"{newLine}User main slogan is: \"{userContext.MainSlogan.Trim()}\"." );
            }

            List<string> habitNames = (userContext.Habits ?? Array.Empty<string>())
                .Where( n => !string.IsNullOrWhiteSpace( n ) )
                .Select( n => n.Trim() )
                .Take( MaxHelperHabits )
                .ToList();
            if (habitNames.Count > 0)
            {
                builder.Append(
                    $"{newLine}A sample of habits the user currently follows " +
                    $"(at most {MaxHelperHabits}; not a full list):{newLine}" );
                builder.Append( string.Join( "; ", habitNames ) );
                builder.Append( '.' );
            }

            List<string> goalNames = (userContext.Goals ?? Array.Empty<string>())
                .Where( n => !string.IsNullOrWhiteSpace( n ) )
                .Select( n => n.Trim() )
                .ToList();
            if (goalNames.Count > 0)
            {
                builder.Append( " My current goals are: " );
                builder.Append( string.Join( "; ", goalNames ) );
                builder.Append( '.' );
            }

            List<string> taskNames = (userContext.Tasks ?? Array.Empty<string>())
                .Where( n => !string.IsNullOrWhiteSpace( n ) )
                .Select( n => n.Trim() )
                .Take( MaxHelperOpenTasks )
                .ToList();
            if (taskNames.Count > 0)
            {
                builder.Append(
                    $"{newLine}A sample of the user's nearest open tasks " +
                    $"(at most {MaxHelperOpenTasks}; not a full inbox): " );
                builder.Append( string.Join( "; ", taskNames ) );
                builder.Append( '.' );
            }
        }

        builder.Append(
            $"{newLine}If you generate a program code, it must be without ``` delimiters. " +
            "The programming language should be specified on a separate line before the code." );

        return builder.ToString();
    }

    private ChatClientAgent CreateHelperAgent( string instructions )
    {
        ChatOptions options = CreateChatOptions(
            jsonObject: false,
            ChatMaxCompletionTokens );
        options.Instructions = instructions;

        return new ChatClientAgent(
            m_chatClient,
            new ChatClientAgentOptions
            {
                Name = "principles-helper",
                Description = "Self-development helper inside the Principles app (goals, habits, tasks).",
                ChatOptions = options
            } );
    }

    private ChatOptions CreateChatOptions( bool jsonObject, int maxCompletionTokens )
    {
        string model = ResolveModel();
        ChatOptions options = new()
        {
            ModelId = model,
            MaxOutputTokens = maxCompletionTokens,
            ResponseFormat = jsonObject ? ChatResponseFormat.Json : null
        };

        if (model.StartsWith( "gpt-5", StringComparison.OrdinalIgnoreCase ))
        {
            // Preserve prior Chat Completions `reasoning_effort: minimal` for gpt-5*.
#pragma warning disable OPENAI001 // ReasoningEffortLevel is experimental in the OpenAI SDK.
            options.RawRepresentationFactory = _ => new OpenAI.Chat.ChatCompletionOptions
            {
                ReasoningEffortLevel = new OpenAI.Chat.ChatReasoningEffortLevel( "minimal" )
            };
#pragma warning restore OPENAI001
        }

        return options;
    }

    private static List<ChatMessage> ToChatMessages( IReadOnlyList<AiChatMessage> messages )
    {
        List<ChatMessage> result = new( messages.Count );
        foreach (AiChatMessage message in messages)
        {
            ChatRole role = message.Role.ToLowerInvariant() switch
            {
                "system" => ChatRole.System,
                "assistant" => ChatRole.Assistant,
                "tool" => ChatRole.Tool,
                _ => ChatRole.User
            };

            IReadOnlyList<AiChatAttachment> attachments = message.Attachments
                ?? Array.Empty<AiChatAttachment>();
            if (attachments.Count == 0)
            {
                result.Add( new ChatMessage( role, message.Content ) );
                continue;
            }

            List<AIContent> parts = new();
            StringBuilder text = new();
            if (!string.IsNullOrWhiteSpace( message.Content ))
            {
                text.Append( message.Content.Trim() );
            }

            foreach (AiChatAttachment attachment in attachments)
            {
                if (attachment?.Data is null || attachment.Data.Length == 0)
                {
                    if (!string.IsNullOrWhiteSpace( attachment?.FileName ))
                    {
                        if (text.Length > 0)
                        {
                            text.AppendLine();
                        }

                        text.Append( "[Attached: " )
                            .Append( attachment!.FileName )
                            .Append( ']' );
                    }

                    continue;
                }

                string mime = (attachment.MimeType ?? string.Empty).Trim().ToLowerInvariant();
                if (mime.StartsWith( "text/", StringComparison.Ordinal ) ||
                    mime is "application/json" or "application/xml" or "application/rtf")
                {
                    string fileText = Encoding.UTF8.GetString( attachment.Data );
                    if (fileText.Length > MaxAttachmentTextChars)
                    {
                        fileText = fileText[..MaxAttachmentTextChars];
                    }

                    if (text.Length > 0)
                    {
                        text.AppendLine();
                    }

                    text.Append( "--- File: " )
                        .Append( string.IsNullOrWhiteSpace( attachment.FileName )
                            ? "attachment"
                            : attachment.FileName )
                        .AppendLine( " ---" )
                        .Append( fileText );
                    continue;
                }

                parts.Add( new DataContent( attachment.Data, string.IsNullOrWhiteSpace( mime )
                    ? "application/octet-stream"
                    : mime ) );
            }

            if (text.Length > 0)
            {
                parts.Insert( 0, new TextContent( text.ToString() ) );
            }

            if (parts.Count == 0)
            {
                continue;
            }

            result.Add( new ChatMessage( role, parts ) );
        }

        return result;
    }

    internal static IReadOnlyList<AiChatAttachment> SanitizeAttachments(
        IReadOnlyList<AiChatAttachment>? attachments )
    {
        if (attachments is null || attachments.Count == 0)
        {
            return Array.Empty<AiChatAttachment>();
        }

        List<AiChatAttachment> sanitized = new();
        foreach (AiChatAttachment attachment in attachments)
        {
            if (sanitized.Count >= MaxAttachmentsPerMessage)
            {
                break;
            }

            if (attachment?.Data is null ||
                attachment.Data.Length == 0 ||
                attachment.Data.Length > MaxAttachmentBytes)
            {
                if (attachment is not null &&
                    !string.IsNullOrWhiteSpace( attachment.FileName ) &&
                    (attachment.Data is null || attachment.Data.Length == 0))
                {
                    sanitized.Add( new AiChatAttachment(
                        attachment.FileName.Trim(),
                        (attachment.MimeType ?? string.Empty).Trim(),
                        Array.Empty<byte>() ) );
                }

                continue;
            }

            string mime = (attachment.MimeType ?? string.Empty).Trim().ToLowerInvariant();
            if (!IsAllowedMime( mime ))
            {
                continue;
            }

            string fileName = string.IsNullOrWhiteSpace( attachment.FileName )
                ? "attachment"
                : attachment.FileName.Trim();
            sanitized.Add( new AiChatAttachment( fileName, mime, attachment.Data ) );
        }

        return sanitized;
    }

    private static bool IsAllowedMime( string mime )
    {
        return mime is
            "image/jpeg" or
            "image/png" or
            "image/gif" or
            "image/webp" or
            "image/heic" or
            "image/heif" or
            "application/pdf" or
            "text/plain" or
            "text/markdown" or
            "text/csv" or
            "text/html" or
            "text/xml" or
            "application/json" or
            "application/xml" or
            "application/rtf" or
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
    }

    private async Task<string> CompleteAsync(
        IReadOnlyList<AiChatMessage> messages,
        bool jsonObject,
        int maxCompletionTokens,
        CancellationToken cancellationToken )
    {
        ResolveApiKey();
        ChatOptions options = CreateChatOptions( jsonObject, maxCompletionTokens );

        ChatResponse response;
        try
        {
            response = await m_chatClient
                .GetResponseAsync( ToChatMessages( messages ), options, cancellationToken )
                .DefaultConfigureAwait();
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new AiServiceException( "AI request timed out.", 504 );
        }
        catch (HttpRequestException ex)
        {
            Log.Error( ex, "OpenAI HTTP request failed" );
            throw new AiServiceException( "Could not reach the AI service.", 502 );
        }
        catch (ClientResultException ex)
        {
            throw MapClientResultException( ex );
        }

        string content = response.Text?.Trim() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace( content ))
        {
            return content;
        }

        string? finish = response.FinishReason?.ToString();
        throw new AiServiceException(
            string.Equals( finish, "Length", StringComparison.OrdinalIgnoreCase ) ||
            string.Equals( finish, "length", StringComparison.OrdinalIgnoreCase )
                ? "The model ran out of tokens before finishing. Try a shorter request."
                : "AI returned an empty response.",
            502 );
    }

    public static IChatClient CreateChatClient( IConfiguration configuration )
    {
        // Allow host startup without AI configured; ResolveApiKey still gates requests.
        string apiKey = FirstNonEmpty(
            configuration["AI_API_KEY"],
            configuration["OpenAi:ApiKey"],
            configuration["AiApiKey"] ) ?? "missing";
        if (apiKey == "123321")
        {
            apiKey = "missing";
        }

        string model = ResolveModel( configuration );
        Uri endpoint = new( ResolveBaseUrl( configuration ) );

        OpenAIClient openAi = new(
            new ApiKeyCredential( apiKey ),
            new OpenAIClientOptions { Endpoint = endpoint } );

        return openAi.GetChatClient( model ).AsIChatClient();
    }

    private string ResolveApiKey() => ResolveApiKey( m_configuration );

    private static string ResolveApiKey( IConfiguration configuration )
    {
        string? apiKey = FirstNonEmpty(
            configuration["AI_API_KEY"],
            configuration["OpenAi:ApiKey"],
            configuration["AiApiKey"] );

        if (string.IsNullOrWhiteSpace( apiKey ) || apiKey == "123321")
        {
            throw new AiServiceException( "AI is not configured on the server.", 503 );
        }

        return apiKey!;
    }

    private string ResolveModel() => ResolveModel( m_configuration );

    private static string ResolveModel( IConfiguration configuration )
    {
        return FirstNonEmpty(
            configuration["OPENAI_MODEL"],
            configuration["OpenAi:Model"],
            DefaultModel ) ?? DefaultModel;
    }

    internal static string ResolveBaseUrl( IConfiguration configuration )
    {
        string baseUrl = FirstNonEmpty(
            configuration["OpenAi:BaseUrl"],
            "https://api.openai.com/v1/" ) ?? "https://api.openai.com/v1/";

        if (!baseUrl.EndsWith( '/' ))
        {
            baseUrl += "/";
        }

        return baseUrl;
    }

    private static AiServiceException MapClientResultException( ClientResultException ex )
    {
        string body = string.Empty;
        try
        {
            PipelineResponse? raw = ex.GetRawResponse();
            if (raw is not null)
            {
                body = raw.Content.ToString() ?? string.Empty;
            }
        }
        catch
        {
            // Fall back to exception message below.
        }

        if (string.IsNullOrWhiteSpace( body ))
        {
            body = ex.Message;
        }

        return MapOpenAiError( ex.Status, body );
    }

    private static AiServiceException MapOpenAiError( int statusCode, string body )
    {
        string? apiMessage = TryReadOpenAiErrorMessage( body );
        string lower = (apiMessage ?? string.Empty).ToLowerInvariant();

        Log.Warning( "OpenAI error status={Status} message={Message}", statusCode, apiMessage );

        if (statusCode == 429 || lower.Contains( "quota" ) || lower.Contains( "rate limit" ))
        {
            return new AiServiceException( "OpenAI quota exceeded.", 429 );
        }

        if (statusCode is 401 or 403)
        {
            return new AiServiceException( "AI is not configured on the server.", 503 );
        }

        return new AiServiceException(
            string.IsNullOrWhiteSpace( apiMessage ) ? "AI request failed." : apiMessage!,
            statusCode >= 400 && statusCode < 600 ? statusCode : 502 );
    }

    private static string? TryReadOpenAiErrorMessage( string body )
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse( body );
            if (document.RootElement.TryGetProperty( "error", out JsonElement error ))
            {
                if (error.ValueKind == JsonValueKind.Object &&
                    error.TryGetProperty( "message", out JsonElement message ))
                {
                    return message.GetString();
                }

                return error.ToString();
            }
        }
        catch (JsonException)
        {
            // ignore
        }

        return null;
    }


    private static string ExtractJsonObject( string content )
    {
        string trimmed = StripMarkdownFences( content );
        int start = trimmed.IndexOf( '{' );
        int end = trimmed.LastIndexOf( '}' );
        if (start >= 0 && end > start)
        {
            return trimmed[start..(end + 1)];
        }

        return trimmed;
    }

    private static string ExtractJsonPayload( string content )
    {
        string trimmed = StripMarkdownFences( content );
        int objectStart = trimmed.IndexOf( '{' );
        int arrayStart = trimmed.IndexOf( '[' );
        if (objectStart >= 0 && (arrayStart < 0 || objectStart < arrayStart))
        {
            int end = trimmed.LastIndexOf( '}' );
            if (end > objectStart)
            {
                return trimmed[objectStart..(end + 1)];
            }
        }

        if (arrayStart >= 0)
        {
            int end = trimmed.LastIndexOf( ']' );
            if (end > arrayStart)
            {
                return trimmed[arrayStart..(end + 1)];
            }
        }

        return trimmed;
    }

    private static string StripMarkdownFences( string content )
    {
        string trimmed = content.Trim();
        if (trimmed.StartsWith( "```", StringComparison.Ordinal ))
        {
            trimmed = Regex.Replace( trimmed, @"^```(?:json)?\s*", string.Empty, RegexOptions.IgnoreCase );
            trimmed = Regex.Replace( trimmed, @"\s*```$", string.Empty );
            trimmed = trimmed.Trim();
        }

        return trimmed;
    }

    private static bool TryGetPropertyIgnoreCase( JsonElement element, string name, out JsonElement value )
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (JsonProperty property in element.EnumerateObject())
            {
                if (string.Equals( property.Name, name, StringComparison.OrdinalIgnoreCase ))
                {
                    value = property.Value;
                    return true;
                }
            }
        }

        value = default;
        return false;
    }

    private static string? ReadStringIgnoreCase( JsonElement element, string name )
    {
        if (!TryGetPropertyIgnoreCase( element, name, out JsonElement value ))
        {
            return null;
        }

        return value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();
    }

    private static string LanguageName( string? culture )
    {
        string raw = (culture ?? string.Empty).Trim().ToLowerInvariant();
        if (raw.StartsWith( "uk" ))
        {
            return "Ukrainian";
        }

        if (raw.StartsWith( "en" ))
        {
            return "English";
        }

        return string.IsNullOrWhiteSpace( culture ) ? "Ukrainian" : culture.Trim();
    }

    private static string? NormalizePriority( string? value )
    {
        if (string.IsNullOrWhiteSpace( value ))
        {
            return null;
        }

        string raw = value.Trim().ToLowerInvariant();
        if (raw is "null" or "none")
        {
            return null;
        }

        if (raw.Contains( "high" ) || raw.Contains( "urgent" ))
        {
            return "high";
        }

        if (raw.Contains( "medium" ) || raw.Contains( "normal" ))
        {
            return "medium";
        }

        if (raw.Contains( "low" ))
        {
            return "low";
        }

        return null;
    }

    private static string ResolveLocalDate( string? localDate, int? utcOffsetMinutes )
    {
        string? provided = NullIfEmpty( localDate );
        if (provided is not null &&
            DateOnly.TryParseExact(
                provided,
                "yyyy-MM-dd",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out DateOnly parsed ))
        {
            return parsed.ToString( "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture );
        }

        DateTime utcNow = DateTime.UtcNow;
        if (utcOffsetMinutes is int offset && offset is >= -14 * 60 and <= 14 * 60)
        {
            return utcNow.AddMinutes( offset ).ToString( "yyyy-MM-dd" );
        }

        return utcNow.ToString( "yyyy-MM-dd" );
    }

    private static string? NormalizeDueDate( string? value )
    {
        string? text = NullIfEmpty( value );
        if (text is null)
        {
            return null;
        }

        // Prefer local wall-clock formats without timezone conversion.
        if (DateTime.TryParseExact(
                text,
                new[]
                {
                    "yyyy-MM-dd",
                    "yyyy-MM-ddTHH:mm",
                    "yyyy-MM-ddTHH:mm:ss",
                    "yyyy-MM-dd HH:mm",
                    "yyyy-MM-dd HH:mm:ss"
                },
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out DateTime exact ))
        {
            return FormatDueDate( exact, hasTime: text.Contains( ':', StringComparison.Ordinal ) );
        }

        if (DateTime.TryParse(
                text,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.RoundtripKind,
                out DateTime parsed ))
        {
            // Drop timezone suffix - treat the clock face as the user's local time.
            bool hasTime = parsed.TimeOfDay != TimeSpan.Zero ||
                text.Contains( 'T', StringComparison.Ordinal ) ||
                text.Contains( ':', StringComparison.Ordinal );
            return FormatDueDate( parsed, hasTime );
        }

        return null;
    }

    private static string FormatDueDate( DateTime value, bool hasTime )
    {
        if (!hasTime)
        {
            return value.ToString( "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture );
        }

        return value.ToString( "yyyy-MM-dd'T'HH:mm", System.Globalization.CultureInfo.InvariantCulture );
    }

    private static IReadOnlyList<int> NormalizeReminders( JsonElement element )
    {
        if (element.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return Array.Empty<int>();
        }

        HashSet<int> unique = new();
        List<int> result = new();

        void Add( int minutes )
        {
            if (minutes < 0 || minutes > 60 * 24 * 30)
            {
                return;
            }

            if (unique.Add( minutes ))
            {
                result.Add( minutes );
            }
        }

        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement item in element.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.Number && item.TryGetInt32( out int minutes ))
                {
                    Add( minutes );
                    continue;
                }

                if (item.ValueKind == JsonValueKind.Object)
                {
                    if (item.TryGetProperty( "offsetMinutes", out JsonElement offset ) ||
                        item.TryGetProperty( "OffsetMinutes", out offset ))
                    {
                        if (offset.ValueKind == JsonValueKind.Number && offset.TryGetInt32( out int value ))
                        {
                            Add( value );
                        }
                    }
                }
            }
        }
        else if (element.ValueKind == JsonValueKind.Number && element.TryGetInt32( out int single ))
        {
            Add( single );
        }

        result.Sort();
        return result;
    }

    private static IReadOnlyList<string> NormalizeSubtasks( JsonElement element )
    {
        if (element.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return Array.Empty<string>();
        }

        if (element.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<string>();
        }

        List<string> result = new();
        foreach (JsonElement item in element.EnumerateArray())
        {
            string? title = null;
            if (item.ValueKind == JsonValueKind.String)
            {
                title = item.GetString();
            }
            else if (item.ValueKind == JsonValueKind.Object)
            {
                if (item.TryGetProperty( "title", out JsonElement titleEl ) ||
                    item.TryGetProperty( "Title", out titleEl ) ||
                    item.TryGetProperty( "name", out titleEl ) ||
                    item.TryGetProperty( "Name", out titleEl ))
                {
                    title = titleEl.ValueKind == JsonValueKind.String ? titleEl.GetString() : titleEl.ToString();
                }
            }

            title = NullIfEmpty( title );
            if (title is null)
            {
                continue;
            }

            if (title.Length > 200)
            {
                title = title[..200];
            }

            result.Add( title );
            if (result.Count >= 30)
            {
                break;
            }
        }

        return result;
    }

    private static string? NullIfEmpty( string? value )
    {
        if (string.IsNullOrWhiteSpace( value ))
        {
            return null;
        }

        string trimmed = value.Trim();
        return trimmed.Equals( "null", StringComparison.OrdinalIgnoreCase ) ? null : trimmed;
    }

    private static string? FirstNonEmpty( params string?[] values )
    {
        foreach (string? value in values)
        {
            if (!string.IsNullOrWhiteSpace( value ))
            {
                return value.Trim();
            }
        }

        return null;
    }

    private sealed class TaskDraftJson
    {
        [JsonPropertyName( "title" )]
        public string? Title { get; set; }

        [JsonPropertyName( "description" )]
        public string? Description { get; set; }

        [JsonPropertyName( "priority" )]
        public string? Priority { get; set; }

        [JsonPropertyName( "theme" )]
        public string? Theme { get; set; }

        [JsonPropertyName( "dueDate" )]
        public string? DueDate { get; set; }

        [JsonPropertyName( "due_date" )]
        public string? DueDateSnake { get; set; }

        [JsonPropertyName( "allDay" )]
        public bool? AllDay { get; set; }

        [JsonPropertyName( "reminders" )]
        public JsonElement Reminders { get; set; }

        [JsonPropertyName( "subtasks" )]
        public JsonElement Subtasks { get; set; }
    }
}
