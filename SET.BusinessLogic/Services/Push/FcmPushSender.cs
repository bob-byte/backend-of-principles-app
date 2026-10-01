using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;

namespace BusinessLogic;

/// <summary>
/// Sends data-only messages through the FCM HTTP v1 API. On Apple platforms FCM
/// relays them to APNs as background pushes (<c>content-available</c>).
/// </summary>
public sealed class FcmPushSender : IPushSender
{
    private readonly HttpClient m_httpClient;
    private readonly FcmCredentials m_credentials;

    public FcmPushSender( HttpClient httpClient, FcmCredentials credentials )
    {
        m_httpClient = httpClient;
        m_credentials = credentials;
    }

    public bool IsConfigured => m_credentials.IsConfigured;

    public async Task<PushSendResult> SendDataAsync(
        string token,
        IReadOnlyDictionary<string, string> data,
        CancellationToken cancellationToken = default )
    {
        if (!IsConfigured)
        {
            return PushSendResult.NotConfigured;
        }

        string accessToken = await m_credentials.GetAccessTokenAsync( cancellationToken ).ConfigureAwait( false );

        using HttpRequestMessage request = new(
            HttpMethod.Post,
            $"https://fcm.googleapis.com/v1/projects/{m_credentials.ProjectId}/messages:send" );
        request.Headers.Authorization = new AuthenticationHeaderValue( "Bearer", accessToken );
        request.Content = JsonContent.Create( BuildMessage( token, data ) );

        using HttpResponseMessage response = await m_httpClient
            .SendAsync( request, cancellationToken )
            .ConfigureAwait( false );
        if (response.IsSuccessStatusCode)
        {
            return PushSendResult.Sent;
        }

        string body = await response.Content.ReadAsStringAsync( cancellationToken ).ConfigureAwait( false );
        if (IsInvalidToken( response.StatusCode, body ))
        {
            return PushSendResult.InvalidToken;
        }

        Log.Warning( "FCM send failed with {StatusCode}: {Body}", (int)response.StatusCode, body );
        return PushSendResult.Failed;
    }

    internal static object BuildMessage( string token, IReadOnlyDictionary<string, string> data ) => new
    {
        message = new
        {
            token,
            data,
            android = new { priority = "HIGH", ttl = "3600s" },
            apns = new
            {
                headers = new Dictionary<string, string>
                {
                    ["apns-push-type"] = "background",
                    // Background pushes must use priority 5; APNs rejects 10 without an alert.
                    ["apns-priority"] = "5",
                },
                payload = new
                {
                    aps = new Dictionary<string, object> { ["content-available"] = 1 },
                },
            },
        },
    };

    internal static bool IsInvalidToken( HttpStatusCode status, string body ) =>
        status == HttpStatusCode.NotFound ||
        body.Contains( "UNREGISTERED", StringComparison.Ordinal ) ||
        body.Contains( "SENDER_ID_MISMATCH", StringComparison.Ordinal ) ||
        (status == HttpStatusCode.BadRequest &&
         body.Contains( "registration token", StringComparison.OrdinalIgnoreCase ));
}
