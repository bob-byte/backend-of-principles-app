using System.Threading;

namespace BusinessLogic;

public interface IEmailSender
{
    Task SendPlainTextAsync( string toEmail, string subject, string body, CancellationToken cancellationToken = default );

    /// <summary>
    /// Sends a multipart message with HTML plus a plain-text fallback for clients that ignore HTML.
    /// </summary>
    Task SendAsync(
        string toEmail,
        string subject,
        string plainTextBody,
        string htmlBody,
        CancellationToken cancellationToken = default );
}
