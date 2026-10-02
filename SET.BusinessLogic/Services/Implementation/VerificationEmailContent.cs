using System.Globalization;
using System.Net;
using System.Text;

namespace BusinessLogic;

/// <summary>
/// Branded HTML + plain-text bodies for account verification codes.
/// </summary>
public static class VerificationEmailContent
{
    public enum Purpose
    {
        PasswordReset,
        Signup
    }

    public sealed record Message( string Subject, string PlainTextBody, string HtmlBody );

    public static Message Build( Purpose purpose, int code )
    {
        string codeText = code.ToString( "D6", CultureInfo.InvariantCulture );
        string formattedCode = $"{codeText[..3]} {codeText[3..]}";

        return purpose switch
        {
            Purpose.Signup => BuildSignup( formattedCode, codeText ),
            Purpose.PasswordReset => BuildPasswordReset( formattedCode, codeText ),
            _ => throw new ArgumentOutOfRangeException( nameof( purpose ), purpose, null )
        };
    }

    private static Message BuildSignup( string formattedCode, string codeText )
    {
        const string subject = "Your Principles signup code";
        const string headline = "Confirm your email";
        const string intro =
            "Welcome to Principles. Enter this code in the app to finish creating your account:";
        const string securityNote =
            "If you did not try to sign up for Principles, you can ignore this email — no account will be created.";

        return new Message(
            Subject: subject,
            PlainTextBody: BuildPlainText( headline, intro, formattedCode, securityNote ),
            HtmlBody: BuildHtml( headline, intro, formattedCode, codeText, securityNote ) );
    }

    private static Message BuildPasswordReset( string formattedCode, string codeText )
    {
        const string subject = "Your Principles password reset code";
        const string headline = "Reset your password";
        const string intro =
            "Enter this code in the Principles app to continue resetting your password:";
        const string securityNote =
            "If you did not request a password reset, you can ignore this email. Your password will stay the same.";

        return new Message(
            Subject: subject,
            PlainTextBody: BuildPlainText( headline, intro, formattedCode, securityNote ),
            HtmlBody: BuildHtml( headline, intro, formattedCode, codeText, securityNote ) );
    }

    private static string BuildPlainText(
        string headline,
        string intro,
        string formattedCode,
        string securityNote )
    {
        StringBuilder sb = new();
        sb.AppendLine( "Principles" );
        sb.AppendLine();
        sb.AppendLine( headline );
        sb.AppendLine();
        sb.AppendLine( intro );
        sb.AppendLine();
        sb.AppendLine( formattedCode );
        sb.AppendLine();
        sb.AppendLine( "This code works only in the Principles app. Do not share it with anyone." );
        sb.AppendLine();
        sb.AppendLine( securityNote );
        sb.AppendLine();
        sb.AppendLine( "- The Principles team" );
        sb.AppendLine( "https://principles.top" );
        return sb.ToString();
    }

    private static string BuildHtml(
        string headline,
        string intro,
        string formattedCode,
        string codeText,
        string securityNote )
    {
        // Email-safe layout: tables + inline CSS. Brand accent matches app primary orange.
        string safeHeadline = WebUtility.HtmlEncode( headline );
        string safeIntro = WebUtility.HtmlEncode( intro );
        string safeFormattedCode = WebUtility.HtmlEncode( formattedCode );
        string safeCodeText = WebUtility.HtmlEncode( codeText );
        string safeSecurityNote = WebUtility.HtmlEncode( securityNote );

        StringBuilder html = new( capacity: 4096 );
        html.Append( "<!DOCTYPE html><html lang=\"en\"><head>" );
        html.Append( "<meta charset=\"utf-8\">" );
        html.Append( "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">" );
        html.Append( "<meta name=\"color-scheme\" content=\"light\">" );
        html.Append( "<title>" ).Append( safeHeadline ).Append( "</title></head>" );
        html.Append( "<body style=\"margin:0;padding:0;background-color:#F8F4EE;" );
        html.Append( "font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;color:#18130F;\">" );
        html.Append( "<table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" " );
        html.Append( "style=\"background-color:#F8F4EE;padding:32px 16px;\"><tr><td align=\"center\">" );
        html.Append( "<table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" " );
        html.Append( "style=\"max-width:480px;background-color:#FFFFFF;border-radius:16px;overflow:hidden;border:1px solid #E8E0D6;\">" );
        html.Append( "<tr><td style=\"height:6px;background-color:#FF6B00;font-size:0;line-height:0;\">&nbsp;</td></tr>" );
        html.Append( "<tr><td style=\"padding:28px 28px 8px 28px;\">" );
        html.Append( "<p style=\"margin:0;font-size:13px;font-weight:600;letter-spacing:0.08em;" );
        html.Append( "text-transform:uppercase;color:#C45600;\">Principles</p>" );
        html.Append( "<h1 style=\"margin:12px 0 0 0;font-size:24px;line-height:1.25;font-weight:700;color:#18130F;\">" );
        html.Append( safeHeadline ).Append( "</h1></td></tr>" );
        html.Append( "<tr><td style=\"padding:12px 28px 8px 28px;\">" );
        html.Append( "<p style=\"margin:0;font-size:16px;line-height:1.5;color:#3A342E;\">" );
        html.Append( safeIntro ).Append( "</p></td></tr>" );
        html.Append( "<tr><td style=\"padding:20px 28px;\">" );
        html.Append( "<table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" " );
        html.Append( "style=\"background-color:#FFF7F0;border:1px solid #FFD7B0;border-radius:12px;\">" );
        html.Append( "<tr><td align=\"center\" style=\"padding:22px 16px;\">" );
        html.Append( "<p style=\"margin:0 0 8px 0;font-size:12px;font-weight:600;letter-spacing:0.06em;" );
        html.Append( "text-transform:uppercase;color:#C45600;\">Verification code</p>" );
        html.Append( "<p style=\"margin:0;font-size:36px;line-height:1.2;font-weight:700;letter-spacing:0.28em;" );
        html.Append( "font-family:ui-monospace,SFMono-Regular,Menlo,Consolas,monospace;color:#18130F;\" " );
        html.Append( "aria-label=\"Verification code " ).Append( safeCodeText ).Append( "\">" );
        html.Append( safeFormattedCode ).Append( "</p></td></tr></table></td></tr>" );
        html.Append( "<tr><td style=\"padding:0 28px 8px 28px;\">" );
        html.Append( "<p style=\"margin:0;font-size:14px;line-height:1.5;color:#6B635A;\">" );
        html.Append( "Enter this code in the Principles app. Do not share it with anyone.</p></td></tr>" );
        html.Append( "<tr><td style=\"padding:16px 28px 28px 28px;\">" );
        html.Append( "<p style=\"margin:0;font-size:13px;line-height:1.5;color:#8A8178;\">" );
        html.Append( safeSecurityNote ).Append( "</p></td></tr>" );
        html.Append( "<tr><td style=\"padding:0 28px 24px 28px;border-top:1px solid #F0EAE2;\">" );
        html.Append( "<p style=\"margin:20px 0 0 0;font-size:12px;line-height:1.5;color:#8A8178;\">" );
        html.Append( "- The Principles team<br>" );
        html.Append( "<a href=\"https://principles.top\" style=\"color:#C45600;text-decoration:none;\">principles.top</a>" );
        html.Append( "</p></td></tr></table></td></tr></table></body></html>" );
        return html.ToString();
    }
}
