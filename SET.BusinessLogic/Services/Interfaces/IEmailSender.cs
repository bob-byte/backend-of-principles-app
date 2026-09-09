using System.Threading;

namespace BusinessLogic;

public interface IEmailSender
{
    Task SendPlainTextAsync( string toEmail, string subject, string body, CancellationToken cancellationToken = default );
}
