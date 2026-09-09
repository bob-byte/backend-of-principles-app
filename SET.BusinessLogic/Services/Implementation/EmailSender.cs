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

    public async Task SendPlainTextAsync(
        string toEmail,
        string subject,
        string body,
        CancellationToken cancellationToken = default )
    {
        string fromEmail = ResolveFromEmail();
        string fromPassword = ResolvePassword();
        string host = m_configuration["Smtp:Host"] ?? "smtp.gmail.com";
        int port = int.TryParse( m_configuration["Smtp:Port"], out int configuredPort )
            ? configuredPort
            : 587;
        string fromName = m_configuration["Smtp:FromName"] ?? "Principles app";

        SecureSocketOptions socketOptions = port == 465
            ? SecureSocketOptions.SslOnConnect
            : SecureSocketOptions.StartTls;

        MimeMessage message = new();
        message.From.Add( new MailboxAddress( fromName, fromEmail ) );
        message.To.Add( MailboxAddress.Parse( toEmail ) );
        message.Subject = subject;
        message.Body = new TextPart( "plain" ) { Text = body };

        using SmtpClient client = new();
        await client.ConnectAsync( host, port, socketOptions, cancellationToken ).ConfigureAwait( false );
        await client.AuthenticateAsync( fromEmail, fromPassword, cancellationToken ).ConfigureAwait( false );
        await client.SendAsync( message, cancellationToken ).ConfigureAwait( false );
        await client.DisconnectAsync( quit: true, cancellationToken ).ConfigureAwait( false );
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
