using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;

using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;

namespace BusinessLogic;

/// <summary>
/// Firebase service-account credentials for the FCM HTTP v1 API.
/// Configure one of <c>FIREBASE_CREDENTIALS_JSON</c> (raw or base64 JSON) or
/// <c>FIREBASE_CREDENTIALS_PATH</c>; when neither is set, pushes are disabled.
/// </summary>
public sealed class FcmCredentials
{
    private const string MessagingScope = "https://www.googleapis.com/auth/firebase.messaging";

    private readonly Lazy<(GoogleCredential? Credential, string? ProjectId)> m_value;

    public FcmCredentials( IConfiguration configuration )
    {
        m_value = new Lazy<(GoogleCredential?, string?)>(
            () => Load( configuration ),
            LazyThreadSafetyMode.ExecutionAndPublication );
    }

    public bool IsConfigured =>
        m_value.Value.Credential is not null && !string.IsNullOrWhiteSpace( m_value.Value.ProjectId );

    public string? ProjectId => m_value.Value.ProjectId;

    public Task<string> GetAccessTokenAsync( CancellationToken cancellationToken ) =>
        ((ITokenAccess)m_value.Value.Credential!).GetAccessTokenForRequestAsync(
            cancellationToken: cancellationToken );

    private static (GoogleCredential?, string?) Load( IConfiguration configuration )
    {
        try
        {
            string? json = ReadJson( configuration );
            if (string.IsNullOrWhiteSpace( json ))
            {
                Log.Information( "FCM credentials are not configured; sync pushes are disabled" );
                return (null, null);
            }

            string? projectId = FirstNonEmpty( configuration["Firebase:ProjectId"], configuration["FIREBASE_PROJECT_ID"] );
            if (projectId is null)
            {
                using JsonDocument document = JsonDocument.Parse( json );
                if (document.RootElement.TryGetProperty( "project_id", out JsonElement id ))
                {
                    projectId = id.GetString();
                }
            }

            GoogleCredential credential = GoogleCredential.FromJson( json ).CreateScoped( MessagingScope );
            return (credential, projectId);
        }
        catch (Exception ex)
        {
            Log.Error( ex, "FCM credentials could not be loaded; sync pushes are disabled" );
            return (null, null);
        }
    }

    private static string? ReadJson( IConfiguration configuration )
    {
        string? raw = FirstNonEmpty(
            configuration["Firebase:CredentialsJson"],
            configuration["FIREBASE_CREDENTIALS_JSON"] );
        if (raw is not null)
        {
            raw = raw.Trim();
            return raw.StartsWith( '{' ) ? raw : Encoding.UTF8.GetString( Convert.FromBase64String( raw ) );
        }

        string? path = FirstNonEmpty(
            configuration["Firebase:CredentialsPath"],
            configuration["FIREBASE_CREDENTIALS_PATH"] );
        return path is not null && File.Exists( path ) ? File.ReadAllText( path ) : null;
    }

    private static string? FirstNonEmpty( params string?[] values ) =>
        values.FirstOrDefault( v => !string.IsNullOrWhiteSpace( v ) );
}
