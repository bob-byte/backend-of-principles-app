using System.Security.Claims;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace SET.WebAPI.Helpers;

/// <summary>
/// Logs each API call with method/path and the caller's email after the response has been sent.
/// </summary>
public sealed class RequestCallerLoggingMiddleware
{
    private static readonly HashSet<string> SilentEmails = new( StringComparer.OrdinalIgnoreCase )
    {
        "batsbohdan@gmail.com",
        "bac.bogdan222@gmail.com",
    };

    private readonly RequestDelegate m_next;

    public RequestCallerLoggingMiddleware( RequestDelegate next )
    {
        m_next = next ?? throw new ArgumentNullException( nameof( next ) );
    }

    public async Task InvokeAsync( HttpContext context, IServiceScopeFactory scopeFactory )
    {
        PathString path = context.Request.Path;
        if (ShouldSkip( path ))
        {
            await m_next( context ).ConfigureAwait( false );
            return;
        }

        var state = new RequestLogState
        {
            Endpoint = $"{context.Request.Method} {path}{context.Request.QueryString.Value}",
            ScopeFactory = scopeFactory,
        };

        context.Response.OnCompleted( WriteLogAfterResponseAsync, state );

        try
        {
            await m_next( context ).ConfigureAwait( false );
        }
        finally
        {
            // Runs before OnCompleted; keep this sync/cheap so it does not delay the response.
            state.StatusCode = context.Response.StatusCode;
            CaptureCaller( context.User, state );
        }
    }

    private static bool ShouldSkip( PathString path )
    {
        string value = path.Value ?? string.Empty;
        return value.StartsWith( "/swagger", StringComparison.OrdinalIgnoreCase )
            || value.Equals( "/favicon.ico", StringComparison.OrdinalIgnoreCase );
    }

    private static void CaptureCaller( ClaimsPrincipal? principal, RequestLogState state )
    {
        if (principal?.Identity?.IsAuthenticated != true)
        {
            state.Email = "anonymous";
            return;
        }

        string? email = principal.FindFirst( ClaimTypes.Email )?.Value
            ?? principal.FindFirst( "email" )?.Value;
        if (!string.IsNullOrWhiteSpace( email ))
        {
            state.Email = email;
            return;
        }

        string? idClaim = principal.FindFirst( ClaimTypes.NameIdentifier )?.Value
            ?? principal.FindFirst( "nameid" )?.Value;
        if (long.TryParse( idClaim, out long userId ) && userId > 0)
        {
            state.UserId = userId;
            return;
        }

        state.Email = "authenticated";
    }

    private static async Task WriteLogAfterResponseAsync( object stateObj )
    {
        var state = (RequestLogState)stateObj;
        string email = state.Email ?? "anonymous";

        if (state.Email is null && state.UserId is long userId)
        {
            try
            {
                using IServiceScope scope = state.ScopeFactory.CreateScope();
                AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                string? fromDb = await dbContext.Users.AsNoTracking()
                    .Where( u => u.Id == userId )
                    .Select( u => u.Email )
                    .FirstOrDefaultAsync()
                    .ConfigureAwait( false );
                email = string.IsNullOrWhiteSpace( fromDb ) ? $"userId:{userId}" : fromDb;
            }
            catch (Exception ex)
            {
                Log.Warning( ex, "Could not resolve email for userId {UserId} after response", userId );
                email = $"userId:{userId}";
            }
        }

        if (SilentEmails.Contains( email ))
        {
            return;
        }

        Log.Information(
            "{Endpoint} called by {Email} → {StatusCode}",
            state.Endpoint,
            email,
            state.StatusCode );
    }

    private sealed class RequestLogState
    {
        public required string Endpoint { get; init; }
        public required IServiceScopeFactory ScopeFactory { get; init; }
        public int StatusCode { get; set; }
        public string? Email { get; set; }
        public long? UserId { get; set; }
    }
}
