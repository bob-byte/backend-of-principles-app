using Microsoft.AspNetCore.Http;
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
    private readonly IAiAssistantService m_aiAssistantService;

    public AiController( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        m_aiAssistantService = serviceProvider.GetRequiredService<IAiAssistantService>();
    }

    [HttpPost( "chat" )]
    public Task<IActionResult> ChatAsync( [FromBody] AiChatRequest request )
    {
        return TryCatchAsync( async user =>
        {
            ServiceResult<AiChatSession> result = await m_aiAssistantService
                .PrepareChatAsync( user, request )
                .DefaultConfigureAwait();
            return ToActionResult( result, () => new ChatSseResult( m_aiAssistantService, result.Value! ) );
        }, request );
    }

    [HttpPost( "parse-task" )]
    public Task<IActionResult> ParseTaskAsync( [FromBody] AiParseTaskRequest request )
    {
        return TryCatchAsync( async _ =>
            ToActionResult( await m_aiAssistantService
                .ParseTaskAsync( request, HttpContext.RequestAborted )
                .DefaultConfigureAwait() ), request );
    }

    [HttpPost( "recommend-habits" )]
    public Task<IActionResult> RecommendHabitsAsync( [FromBody] AiRecommendHabitsRequest request )
    {
        return TryCatchAsync( async user =>
            ToActionResult( await m_aiAssistantService
                .RecommendHabitsAsync( user, request, HttpContext.RequestAborted )
                .DefaultConfigureAwait() ), request );
    }

    [HttpPost( "title" )]
    public Task<IActionResult> TitleAsync( [FromBody] AiTitleRequest request )
    {
        return TryCatchAsync( async _ =>
            ToActionResult( await m_aiAssistantService
                .GenerateTitleAsync( request, HttpContext.RequestAborted )
                .DefaultConfigureAwait() ), request );
    }

    [HttpPost( "suggest-profile-text" )]
    public Task<IActionResult> SuggestProfileTextAsync( [FromBody] AiSuggestProfileTextRequest request )
    {
        return TryCatchAsync( async user =>
            ToActionResult( await m_aiAssistantService
                .SuggestProfileTextAsync( user, request, HttpContext.RequestAborted )
                .DefaultConfigureAwait() ), request );
    }

    [HttpPost( "recommend-goals" )]
    public Task<IActionResult> RecommendGoalsAsync( [FromBody] AiRecommendGoalsRequest request )
    {
        return TryCatchAsync( async user =>
            ToActionResult( await m_aiAssistantService
                .RecommendGoalsAsync( user, request, HttpContext.RequestAborted )
                .DefaultConfigureAwait() ), request );
    }
}

internal sealed class ChatSseResult : IActionResult
{
    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly IAiAssistantService m_aiAssistantService;
    private readonly AiChatSession m_session;

    public ChatSseResult( IAiAssistantService aiAssistantService, AiChatSession session )
    {
        m_aiAssistantService = aiAssistantService;
        m_session = session;
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
            await foreach (string chunk in m_aiAssistantService
                .StreamChatAsync( m_session, cancellationToken )
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
