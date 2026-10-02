using System.Threading;

using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace BusinessLogic;

/// <summary>
/// Sends mail via Gmail SMTP using MailKit.
/// Password must be a Google App Password when 2FA is enabled on the account.
/// </summary>
public sealed class EmailSender : IEmailSender
{
    private readonly IConfiguration m_configuration;

    public EmailSender( IConfiguration configuration )
    {
        m_configuration = configuration;
    }

    public Task SendPlainTextAsync(
        string toEmail,
        string subject,
        string body,
        CancellationToken cancellationToken = default )
    {
        return DeliverAsync( toEmail, subject, BuildPlainBody( body ), cancellationToken );
    }

    public Task SendAsync(
        string toEmail,
        string subject,
        string plainTextBody,
        string htmlBody,
        CancellationToken cancellationToken = default )
    {
        return DeliverAsync( toEmail, subject, BuildMultipartBody( plainTextBody, htmlBody ), cancellationToken );
    }

    private async Task DeliverAsync(
        string toEmail,
        string subject,
        MimeEntity body,
        CancellationToken cancellationToken )
    {
        string fromEmail = ResolveFromEmail();
        string fromPassword = ResolvePassword();
        string host = m_configuration["Smtp:Host"] ?? "smtp.gmail.com";
        int port = int.TryParse( m_configuration["Smtp:Port"], out int configuredPort )
            ? configuredPort
            : 587;
        string fromName = m_configuration["Smtp:FromName"] ?? "Principles";

        SecureSocketOptions socketOptions = port == 465
            ? SecureSocketOptions.SslOnConnect
            : SecureSocketOptions.StartTls;

        MimeMessage message = new();
        message.From.Add( new MailboxAddress( fromName, fromEmail ) );
        message.To.Add( MailboxAddress.Parse( toEmail ) );
        message.Subject = subject;
        message.Body = body;

        using SmtpClient client = new();
#if DEBUG
        // macOS/.NET local trust stores sometimes reject Gmail's chain (revocation
        // checks / custom SslStream callbacks). Keep strict validation in Release.
        client.CheckCertificateRevocation = false;
        client.ServerCertificateValidationCallback = static ( _, _, _, _ ) => true;
#endif
        await client.ConnectAsync( host, port, socketOptions, cancellationToken ).ConfigureAwait( false );
        await client.AuthenticateAsync( fromEmail, fromPassword, cancellationToken ).ConfigureAwait( false );
        await client.SendAsync( message, cancellationToken ).ConfigureAwait( false );
        await client.DisconnectAsync( quit: true, cancellationToken ).ConfigureAwait( false );
    }

    private static TextPart BuildPlainBody( string plainTextBody )
    {
        return new TextPart( "plain" )
        {
            Text = plainTextBody,
            ContentTransferEncoding = ContentEncoding.QuotedPrintable
        };
    }

    private static MultipartAlternative BuildMultipartBody( string plainTextBody, string htmlBody )
    {
        // Plain first, HTML last so capable clients prefer HTML.
        return new MultipartAlternative
        {
            BuildPlainBody( plainTextBody ),
            new TextPart( "html" )
            {
                Text = htmlBody,
                ContentTransferEncoding = ContentEncoding.QuotedPrintable
            }
        };
    }

    private string ResolveFromEmail()
    {
        string? fromEmail = m_configuration["HostEmail"] ?? m_configuration["Smtp:FromEmail"];
        if (string.IsNullOrWhiteSpace( fromEmail ))
        {
            throw new InvalidOperationException( "HostEmail is not configured." );
        }

        return fromEmail.Trim();
    }

    private string ResolvePassword()
    {
        string? password = m_configuration["HostEmailPassword"];
        if (string.IsNullOrWhiteSpace( password ))
        {
            password = m_configuration["HOST_EMAIL_PASSWORD"];
        }

        if (string.IsNullOrWhiteSpace( password ))
        {
            throw new InvalidOperationException(
                "HostEmailPassword / HOST_EMAIL_PASSWORD is not configured. " +
                "For Gmail with 2FA, use a Google App Password." );
        }

        return password;
    }
}
