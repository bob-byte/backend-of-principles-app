using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;

using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;

namespace SET.WebAPI.Controllers;

[Route( "api/ai" )]
[ApiController]
[Authorize( AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme )]
public class AiController : BaseController
{
    private const int MaxPromptChars = 8000;

    private readonly IAiService m_aiService;

    public AiController( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        m_aiService = serviceProvider.GetRequiredService<IAiService>();
    }

    [HttpPost( "chat" )]
    public Task<IActionResult> ChatAsync( [FromBody] AiChatRequest request )
    {
        return TryCatchAsync( async user =>
        {
            List<AiChatMessage> messages = ToChatMessages( request );
            if (messages.Count == 0)
            {
                return BadRequest( new { error = "PromptIsRequired" } );
            }

            ChatUserContext userContext = await BuildChatUserContextAsync( user ).DefaultConfigureAwait();
            return new ChatSseResult( m_aiService, messages, userContext );
        }, request );
    }

    [HttpPost( "parse-task" )]
    public Task<IActionResult> ParseTaskAsync( [FromBody] AiParseTaskRequest request )
    {
        return TryCatchAsync( async _ =>
        {
            string prompt = request?.Prompt?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace( prompt ))
            {
                return BadRequest( new { error = "PromptIsRequired" } );
            }

            if (prompt.Length > MaxPromptChars)
            {
                prompt = prompt[..MaxPromptChars];
            }

            try
            {
                AiTaskDraftResult draft = await m_aiService
                    .ParseTaskDraftAsync( prompt, HttpContext.RequestAborted )
                    .DefaultConfigureAwait();

                return Ok( new AiTaskDraftDto
                {
                    Title = draft.Title,
                    Description = draft.Description,
                    Priority = draft.Priority,
                    Theme = draft.Theme,
                    DueDate = draft.DueDate
                } );
            }
            catch (AiServiceException ex)
            {
                return StatusCode( ex.StatusCode, new { error = ex.Message } );
            }
        }, request );
    }

    [HttpPost( "recommend-habits" )]
    public Task<IActionResult> RecommendHabitsAsync( [FromBody] AiRecommendHabitsRequest request )
    {
        return TryCatchAsync( async user =>
        {
            try
            {
                RecommendHabitsContext context = ToRecommendContext( request, user );
                IReadOnlyList<RecommendedHabitResult> habits = await m_aiService
                    .RecommendHabitsAsync( context, HttpContext.RequestAborted )
                    .DefaultConfigureAwait();

                return Ok( new AiRecommendHabitsResponse
                {
                    Habits = habits.Select( h => new AiRecommendedHabitDto
                    {
                        Name = h.Name,
                        ReasonToFollow = h.ReasonToFollow
                    } ).ToList()
                } );
            }
            catch (AiServiceException ex)
            {
                return StatusCode( ex.StatusCode, new { error = ex.Message } );
            }
        }, request );
    }

    private async Task<ChatUserContext> BuildChatUserContextAsync( User user )
    {
        List<string> habits = await LoadActiveHabitNamesAsync( user.Id ).DefaultConfigureAwait();
        List<string> goals = await LoadGoalNamesAsync( user.Id ).DefaultConfigureAwait();

        return new ChatUserContext
        {
            Name = NullIfEmpty( user.Name ),
            Gender = FormatChatGender( user.Gender ),
            Mission = NullIfEmpty( user.Mission ),
            MainSlogan = NullIfEmpty( user.MainSlogan ),
            Habits = habits,
            Goals = goals
        };
    }

    private static RecommendHabitsContext ToRecommendContext(
        AiRecommendHabitsRequest request,
        User user )
    {
        string culture = string.IsNullOrWhiteSpace( request?.Culture )
            ? "uk"
            : request.Culture.Trim();

        string? goal = NullIfEmpty( request?.Goal );
        List<string> goals = SanitizeNames( request?.Goals );
        List<string> currentHabits = SanitizeNames( request?.CurrentHabits );

        string? mission = NullIfEmpty( request?.Mission ) ?? NullIfEmpty( user.Mission );
        string? slogan = NullIfEmpty( request?.MainSlogan ) ?? NullIfEmpty( user.MainSlogan );
        Gender gender = request?.Gender is int value && Enum.IsDefined( typeof( Gender ), value )
            ? (Gender)value
            : user.Gender;

        return new RecommendHabitsContext
        {
            Culture = culture,
            Goal = goal,
            Goals = goals,
            CurrentHabits = currentHabits,
            Mission = mission,
            MainSlogan = slogan,
            Gender = gender.ToString().ToLowerInvariant()
        };
    }

    private async Task<List<string>> LoadActiveHabitNamesAsync( long userId )
    {
        List<string> names = await DbContext.UserHabits
            .Where( h => h.UserId == userId && !h.IsArchived && h.Status == StatusOfHabit.InProgress )
            .OrderBy( h => h.Priority )
            .Select( h => h.Name )
            .ToListAsync()
            .DefaultConfigureAwait();

        return SanitizeNames( names );
    }

    private async Task<List<string>> LoadGoalNamesAsync( long userId )
    {
        List<string> names = await DbContext.UserGoals
            .Where( g => g.UserId == userId )
            .OrderBy( g => g.Id )
            .Select( g => g.Name )
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
                if (dto is null || string.IsNullOrWhiteSpace( dto.Content ))
                {
                    continue;
                }

                messages.Add( new AiChatMessage(
                    dto.Role ?? "user",
                    dto.Content ) );
            }
        }

        string prompt = request?.Prompt?.Trim() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace( prompt ))
        {
            messages.Add( new AiChatMessage( "user", prompt ) );
        }

        return messages;
    }
}

internal sealed class ChatSseResult : IActionResult
{
    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly IAiService m_aiService;
    private readonly IReadOnlyList<AiChatMessage> m_messages;
    private readonly ChatUserContext? m_userContext;

    public ChatSseResult(
        IAiService aiService,
        IReadOnlyList<AiChatMessage> messages,
        ChatUserContext? userContext = null )
    {
        m_aiService = aiService;
        m_messages = messages;
        m_userContext = userContext;
    }

    public async Task ExecuteResultAsync( ActionContext context )
    {
        HttpResponse response = context.HttpContext.Response;
        CancellationToken cancellationToken = context.HttpContext.RequestAborted;

        response.Headers["Cache-Control"] = "no-cache, no-transform";
        response.Headers["X-Accel-Buffering"] = "no";
        response.ContentType = "text/event-stream; charset=utf-8";
        context.HttpContext.Features.Get<IHttpResponseBodyFeature>()?.DisableBuffering();

        try
        {
            await foreach (string chunk in m_aiService
                .StreamChatAsync( m_messages, m_userContext, cancellationToken )
                .ConfigureAwait( false ))
            {
                await WriteSseAsync(
                    response,
                    new Dictionary<string, string> { ["content"] = chunk },
                    cancellationToken ).DefaultConfigureAwait();
            }

            await response.WriteAsync( "data: [DONE]\n\n", cancellationToken ).DefaultConfigureAwait();
            await response.Body.FlushAsync( cancellationToken ).DefaultConfigureAwait();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Client disconnected.
        }
        catch (AiServiceException ex)
        {
            await WriteFailureAsync( response, ex.StatusCode, ex.Message, cancellationToken )
                .DefaultConfigureAwait();
        }
        catch (Exception ex)
        {
            Log.Error( ex, "AI chat stream failed" );
            await WriteFailureAsync( response, 502, "AI request failed.", cancellationToken )
                .DefaultConfigureAwait();
        }
    }

    private static async Task WriteFailureAsync(
        HttpResponse response,
        int statusCode,
        string message,
        CancellationToken cancellationToken )
    {
        if (!response.HasStarted)
        {
            response.StatusCode = statusCode;
            response.ContentType = "application/json; charset=utf-8";
            string json = JsonSerializer.Serialize(
                new Dictionary<string, string> { ["error"] = message },
                s_jsonOptions );
            await response.WriteAsync( json, cancellationToken ).DefaultConfigureAwait();
            return;
        }

        await WriteSseAsync(
            response,
            new Dictionary<string, string> { ["error"] = message },
            cancellationToken ).DefaultConfigureAwait();
    }

    private static async Task WriteSseAsync(
        HttpResponse response,
        IReadOnlyDictionary<string, string> payload,
        CancellationToken cancellationToken )
    {
        string json = JsonSerializer.Serialize( payload, s_jsonOptions );
        await response.WriteAsync( $"data: {json}\n\n", cancellationToken ).DefaultConfigureAwait();
        await response.Body.FlushAsync( cancellationToken ).DefaultConfigureAwait();
    }
}
