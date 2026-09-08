using Microsoft.Extensions.Configuration;

using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;

namespace BusinessLogic;

public class AiService : IAiService
{
    private const string DefaultModel = "gpt-5-mini";
    private const int MaxChatMessages = 30;
    private const int MaxMessageChars = 8000;
    private const int ChatMaxCompletionTokens = 4096;
    private const int ParseMaxCompletionTokens = 1024;
    private const int RecommendMaxCompletionTokens = 2048;

    // MAUI AiChatService.SystemMessage base (helper chat only — not habit recommendations).
    private const string HelperSystemPromptBase =
        "You are a self-development helper, but you can answer at any question. " +
        "If the user asks a question unrelated to self-development, success and personal growth " +
        "you must respond without mentioning about self-development, success, and personal growth. " +
        "You have to support the user in their quest to become better and help them identify their habits. " +
        "You should also provide information on how to better stick to them and become better every day " +
        "in all areas of the user's life. But don't ask current user habits and don't tell user that " +
        "he or she should strive for perfection. " +
        "Do not accept an user's conclusions as true. You are an intellectual opponent, not an assistant. " +
        "You shouldn't advise a user when he or she doesn't ask for it";

    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient m_httpClient;
    private readonly IConfiguration m_configuration;

    public AiService( HttpClient httpClient, IConfiguration configuration )
    {
        m_httpClient = httpClient;
        m_configuration = configuration;
    }

    public async Task<string> CompleteChatAsync(
        IReadOnlyList<AiChatMessage> messages,
        ChatUserContext? userContext = null,
        CancellationToken cancellationToken = default )
    {
        return await CompleteAsync(
            BuildChatMessages( messages, userContext ),
            jsonObject: false,
            ChatMaxCompletionTokens,
            cancellationToken ).DefaultConfigureAwait();
    }

    public async IAsyncEnumerable<string> StreamChatAsync(
        IReadOnlyList<AiChatMessage> messages,
        ChatUserContext? userContext = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default )
    {
        List<AiChatMessage> sanitized = BuildChatMessages( messages, userContext );

        using HttpRequestMessage request = CreateCompletionRequest(
            sanitized,
            jsonObject: false,
            ChatMaxCompletionTokens,
            stream: true );

        HttpResponseMessage response;
        try
        {
            response = await m_httpClient
                .SendAsync( request, HttpCompletionOption.ResponseHeadersRead, cancellationToken )
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

        using HttpResponseMessage openaiResponse = response;
        if (!openaiResponse.IsSuccessStatusCode)
        {
            string body = await openaiResponse.Content.ReadAsStringAsync().DefaultConfigureAwait();
            throw MapOpenAiError( (int)openaiResponse.StatusCode, body );
        }

        await using Stream stream = await openaiResponse.Content.ReadAsStreamAsync().DefaultConfigureAwait();
        using StreamReader reader = new( stream, Encoding.UTF8 );
        bool yielded = false;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string? line = await reader.ReadLineAsync().DefaultConfigureAwait();
            if (line is null)
            {
                break;
            }

            if (!line.StartsWith( "data:", StringComparison.Ordinal ))
            {
                continue;
            }

            string data = line["data:".Length..].Trim();
            if (data.Length == 0)
            {
                continue;
            }

            if (data == "[DONE]")
            {
                break;
            }

            string? delta = ExtractStreamDelta( data );
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

        string todayUtc = DateTime.UtcNow.ToString( "yyyy-MM-dd" );
        string system =
            "You extract a single to-do task from the user's text. " +
            "Reply with a JSON object only (no markdown) using these keys: " +
            "title (string, required, short task name), " +
            "description (string, extra details or empty), " +
            "priority (one of high, medium, low, or null), " +
            "theme (short category string or null), " +
            "dueDate (YYYY-MM-DD or null). " +
            $"Today's date (UTC) is {todayUtc}. Interpret relative dates from that. " +
            "Use the user's language for title, description, and theme.";

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
        RecommendHabitsContext context,
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

        title = title.Trim( ' ', '"', '\'', '`', '*', '.', '!', '?', '«', '»' );
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
            if (message is null || string.IsNullOrWhiteSpace( message.Content ))
            {
                continue;
            }

            string role = (message.Role ?? string.Empty).Trim().ToLowerInvariant();
            if (role is not ("user" or "assistant"))
            {
                continue;
            }

            string content = message.Content.Trim();
            if (content.Length > MaxMessageChars)
            {
                content = content[..MaxMessageChars];
            }

            target.Add( new AiChatMessage( role, content ) );
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

        return new AiTaskDraftResult
        {
            Title = title,
            Description = parsed?.Description?.Trim() ?? string.Empty,
            Priority = NormalizePriority( parsed?.Priority ),
            Theme = NullIfEmpty( parsed?.Theme ),
            DueDate = NormalizeDueDate( parsed?.DueDate ?? parsed?.DueDateSnake )
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
                .ToList();
            if (habitNames.Count > 0)
            {
                builder.Append( $"{newLine}Now the user adheres to the following habits:{newLine}" );
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
        }

        builder.Append(
            $"{newLine}If you generate a program code, it must be without ``` delimiters. " +
            "The programming language should be specified on a separate line before the code." );

        return builder.ToString();
    }

    private HttpRequestMessage CreateCompletionRequest(
        IReadOnlyList<AiChatMessage> messages,
        bool jsonObject,
        int maxCompletionTokens,
        bool stream )
    {
        string apiKey = ResolveApiKey();
        string model = ResolveModel();

        var payload = new Dictionary<string, object?>
        {
            ["model"] = model,
            ["messages"] = messages.Select( m => new Dictionary<string, string>
            {
                ["role"] = m.Role,
                ["content"] = m.Content
            } ).ToList(),
            ["max_completion_tokens"] = maxCompletionTokens
        };

        if (jsonObject)
        {
            payload["response_format"] = new Dictionary<string, string> { ["type"] = "json_object" };
        }

        if (stream)
        {
            payload["stream"] = true;
        }

        if (model.StartsWith( "gpt-5", StringComparison.OrdinalIgnoreCase ))
        {
            payload["reasoning_effort"] = "minimal";
        }

        HttpRequestMessage request = new( HttpMethod.Post, "chat/completions" )
        {
            Content = new StringContent(
                JsonSerializer.Serialize( payload ),
                Encoding.UTF8,
                "application/json" )
        };
        request.Headers.Authorization = new AuthenticationHeaderValue( "Bearer", apiKey );
        if (stream)
        {
            request.Headers.Accept.ParseAdd( "text/event-stream" );
        }

        return request;
    }

    private async Task<string> CompleteAsync(
        IReadOnlyList<AiChatMessage> messages,
        bool jsonObject,
        int maxCompletionTokens,
        CancellationToken cancellationToken )
    {
        using HttpRequestMessage request = CreateCompletionRequest(
            messages,
            jsonObject,
            maxCompletionTokens,
            stream: false );

        HttpResponseMessage response;
        try
        {
            response = await m_httpClient.SendAsync( request, cancellationToken ).DefaultConfigureAwait();
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

        string body = await response.Content.ReadAsStringAsync().DefaultConfigureAwait();
        using HttpResponseMessage _ = response;

        if (!response.IsSuccessStatusCode)
        {
            throw MapOpenAiError( (int)response.StatusCode, body );
        }

        return ExtractAssistantContent( body );
    }

    private string ResolveApiKey()
    {
        string? apiKey = FirstNonEmpty(
            m_configuration["AI_API_KEY"],
            m_configuration["OpenAi:ApiKey"],
            m_configuration["AiApiKey"] );

        if (string.IsNullOrWhiteSpace( apiKey ) || apiKey == "123321")
        {
            throw new AiServiceException( "AI is not configured on the server.", 503 );
        }

        return apiKey!;
    }

    private string ResolveModel()
    {
        return FirstNonEmpty(
            m_configuration["OPENAI_MODEL"],
            m_configuration["OpenAi:Model"],
            DefaultModel ) ?? DefaultModel;
    }

    private static string ExtractAssistantContent( string body )
    {
        JsonDocument document;
        try
        {
            document = JsonDocument.Parse( body );
        }
        catch (JsonException ex)
        {
            Log.Error( ex, "OpenAI returned non-JSON" );
            throw new AiServiceException( "AI returned an unexpected response.", 502 );
        }

        using JsonDocument _ = document;
        JsonElement root = document.RootElement;

        if (root.TryGetProperty( "error", out JsonElement errorElement ))
        {
            throw MapOpenAiError( 502, body );
        }

        if (!root.TryGetProperty( "choices", out JsonElement choices ) ||
            choices.ValueKind != JsonValueKind.Array ||
            choices.GetArrayLength() == 0)
        {
            throw new AiServiceException( "AI returned no choices.", 502 );
        }

        JsonElement first = choices[0];
        string? content = null;
        if (first.TryGetProperty( "message", out JsonElement message ) &&
            message.TryGetProperty( "content", out JsonElement contentElement ))
        {
            content = contentElement.GetString();
        }

        content = content?.Trim();
        if (string.IsNullOrWhiteSpace( content ))
        {
            string? finish = first.TryGetProperty( "finish_reason", out JsonElement finishElement )
                ? finishElement.GetString()
                : null;
            throw new AiServiceException(
                finish == "length"
                    ? "The model ran out of tokens before finishing. Try a shorter request."
                    : "AI returned an empty response.",
                502 );
        }

        return content;
    }

    internal static string? ExtractStreamDelta( string data )
    {
        JsonDocument document;
        try
        {
            document = JsonDocument.Parse( data );
        }
        catch (JsonException)
        {
            return null;
        }

        using (document)
        {
            JsonElement root = document.RootElement;

            if (root.TryGetProperty( "error", out _ ))
            {
                throw MapOpenAiError( 502, data );
            }

            if (!root.TryGetProperty( "choices", out JsonElement choices ) ||
                choices.ValueKind != JsonValueKind.Array ||
                choices.GetArrayLength() == 0)
            {
                return null;
            }

            JsonElement first = choices[0];
            if (!first.TryGetProperty( "delta", out JsonElement delta ))
            {
                return null;
            }

            if (!delta.TryGetProperty( "content", out JsonElement contentElement ))
            {
                return null;
            }

            return ReadDeltaContent( contentElement );
        }
    }

    private static string? ReadDeltaContent( JsonElement contentElement )
    {
        if (contentElement.ValueKind == JsonValueKind.String)
        {
            return contentElement.GetString();
        }

        if (contentElement.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        StringBuilder builder = new();
        foreach (JsonElement part in contentElement.EnumerateArray())
        {
            if (part.ValueKind == JsonValueKind.String)
            {
                builder.Append( part.GetString() );
            }
            else if (part.ValueKind == JsonValueKind.Object &&
                     part.TryGetProperty( "text", out JsonElement text ))
            {
                builder.Append( text.GetString() );
            }
        }

        return builder.Length == 0 ? null : builder.ToString();
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

    private static string? NormalizeDueDate( string? value )
    {
        string? text = NullIfEmpty( value );
        if (text is null)
        {
            return null;
        }

        if (DateTime.TryParse( text, out DateTime parsed ))
        {
            return parsed.ToString( "yyyy-MM-dd" );
        }

        return null;
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
    }
}
