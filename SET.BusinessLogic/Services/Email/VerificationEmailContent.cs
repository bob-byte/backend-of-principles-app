using System.Globalization;
using System.Net;
using System.Text;

namespace BusinessLogic;

/// <summary>
/// Branded HTML + plain-text bodies for account verification codes (EN / UK).
/// </summary>
public static class VerificationEmailContent
{
    public enum Purpose
    {
        PasswordReset,
        Signup
    }

    public sealed record Message( string Subject, string PlainTextBody, string HtmlBody );

    public static Message Build( Purpose purpose, int code, string? language = null )
    {
        string codeText = code.ToString( "D6", CultureInfo.InvariantCulture );
        string formattedCode = $"{codeText[..3]} {codeText[3..]}";
        bool ukrainian = IsUkrainian( language );
        Copy copy = ukrainian ? UkrainianCopy( purpose ) : EnglishCopy( purpose );
        string htmlLang = ukrainian ? "uk" : "en";

        return new Message(
            Subject: copy.Subject,
            PlainTextBody: BuildPlainText( copy, formattedCode ),
            HtmlBody: BuildHtml( copy, formattedCode, codeText, htmlLang ) );
    }

    internal static bool IsUkrainian( string? language )
    {
        if (string.IsNullOrWhiteSpace( language ))
        {
            return false;
        }

        string normalized = language.Trim().ToLowerInvariant().Replace( '_', '-' );
        return normalized == "uk" ||
               normalized == "ua" ||
               normalized.StartsWith( "uk-", StringComparison.Ordinal ) ||
               normalized.StartsWith( "ua-", StringComparison.Ordinal );
    }

    private static Copy EnglishCopy( Purpose purpose ) => purpose switch
    {
        Purpose.Signup => new Copy(
            Subject: "Your Principles signup code",
            Headline: "Confirm your email",
            Intro: "Welcome to Principles. Enter this code in the app to finish creating your account:",
            CodeLabel: "Verification code",
            ShareWarning: "Enter this code in the Principles app. Do not share it with anyone.",
            SecurityNote:
            "If you did not try to sign up for Principles, you can ignore this email — no account will be created.",
            TeamSignOff: "- The Principles team" ),
        Purpose.PasswordReset => new Copy(
            Subject: "Your Principles password reset code",
            Headline: "Reset your password",
            Intro: "Enter this code in the Principles app to continue resetting your password:",
            CodeLabel: "Verification code",
            ShareWarning: "Enter this code in the Principles app. Do not share it with anyone.",
            SecurityNote:
            "If you did not request a password reset, you can ignore this email. Your password will stay the same.",
            TeamSignOff: "- The Principles team" ),
        _ => throw new ArgumentOutOfRangeException( nameof( purpose ), purpose, null )
    };

    private static Copy UkrainianCopy( Purpose purpose ) => purpose switch
    {
        Purpose.Signup => new Copy(
            Subject: "Ваш код реєстрації Principles",
            Headline: "Підтвердіть електронну пошту",
            Intro:
            "Ласкаво просимо до Principles. Введіть цей код у додатку, щоб завершити створення облікового запису:",
            CodeLabel: "Код підтвердження",
            ShareWarning: "Введіть цей код у додатку Principles. Нікому його не повідомляйте.",
            SecurityNote:
            "Якщо ви не намагалися зареєструватися в Principles, можете проігнорувати цей лист — обліковий запис не буде створено.",
            TeamSignOff: "— Команда Principles" ),
        Purpose.PasswordReset => new Copy(
            Subject: "Ваш код скидання пароля Principles",
            Headline: "Скидання пароля",
            Intro: "Введіть цей код у додатку Principles, щоб продовжити скидання пароля:",
            CodeLabel: "Код підтвердження",
            ShareWarning: "Введіть цей код у додатку Principles. Нікому його не повідомляйте.",
            SecurityNote:
            "Якщо ви не запитували скидання пароля, можете проігнорувати цей лист. Ваш пароль залишиться без змін.",
            TeamSignOff: "— Команда Principles" ),
        _ => throw new ArgumentOutOfRangeException( nameof( purpose ), purpose, null )
    };

    private static string BuildPlainText( Copy copy, string formattedCode )
    {
        StringBuilder sb = new();
        sb.AppendLine( "Principles" );
        sb.AppendLine();
        sb.AppendLine( copy.Headline );
        sb.AppendLine();
        sb.AppendLine( copy.Intro );
        sb.AppendLine();
        sb.AppendLine( formattedCode );
        sb.AppendLine();
        sb.AppendLine( copy.ShareWarning );
        sb.AppendLine();
        sb.AppendLine( copy.SecurityNote );
        sb.AppendLine();
        sb.AppendLine( copy.TeamSignOff );
        sb.AppendLine( "https://principles.top" );
        return sb.ToString();
    }

    private static string BuildHtml(
        Copy copy,
        string formattedCode,
        string codeText,
        string htmlLang )
    {
        // Email-safe layout: tables + inline CSS. Brand accent matches app primary orange.
        string safeHeadline = WebUtility.HtmlEncode( copy.Headline );
        string safeIntro = WebUtility.HtmlEncode( copy.Intro );
        string safeFormattedCode = WebUtility.HtmlEncode( formattedCode );
        string safeCodeText = WebUtility.HtmlEncode( codeText );
        string safeSecurityNote = WebUtility.HtmlEncode( copy.SecurityNote );
        string safeCodeLabel = WebUtility.HtmlEncode( copy.CodeLabel );
        string safeShareWarning = WebUtility.HtmlEncode( copy.ShareWarning );
        string safeTeamSignOff = WebUtility.HtmlEncode( copy.TeamSignOff );

        StringBuilder html = new( capacity: 4200 );
        html.Append( "<!DOCTYPE html><html lang=\"" ).Append( htmlLang ).Append( "\"><head>" );
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
        html.Append( "text-transform:uppercase;color:#C45600;\">" ).Append( safeCodeLabel ).Append( "</p>" );
        // Selectable digits — email clients block clipboard JS, so no copy button.
        html.Append( "<p style=\"margin:0;font-size:36px;line-height:1.2;font-weight:700;letter-spacing:0.28em;" );
        html.Append( "font-family:ui-monospace,SFMono-Regular,Menlo,Consolas,monospace;color:#18130F;" );
        html.Append( "-webkit-user-select:all;user-select:all;\" " );
        html.Append( "aria-label=\"" ).Append( safeCodeLabel ).Append( ' ' ).Append( safeCodeText ).Append( "\">" );
        html.Append( safeFormattedCode ).Append( "</p></td></tr></table></td></tr>" );
        html.Append( "<tr><td style=\"padding:0 28px 8px 28px;\">" );
        html.Append( "<p style=\"margin:0;font-size:14px;line-height:1.5;color:#6B635A;\">" );
        html.Append( safeShareWarning ).Append( "</p></td></tr>" );
        html.Append( "<tr><td style=\"padding:16px 28px 28px 28px;\">" );
        html.Append( "<p style=\"margin:0;font-size:13px;line-height:1.5;color:#8A8178;\">" );
        html.Append( safeSecurityNote ).Append( "</p></td></tr>" );
        html.Append( "<tr><td style=\"padding:0 28px 24px 28px;border-top:1px solid #F0EAE2;\">" );
        html.Append( "<p style=\"margin:20px 0 0 0;font-size:12px;line-height:1.5;color:#8A8178;\">" );
        html.Append( safeTeamSignOff ).Append( "<br>" );
        html.Append( "<a href=\"https://principles.top\" style=\"color:#C45600;text-decoration:none;\">principles.top</a>" );
        html.Append( "</p></td></tr></table></td></tr></table></body></html>" );
        return html.ToString();
    }

    private sealed record Copy(
        string Subject,
        string Headline,
        string Intro,
        string CodeLabel,
        string ShareWarning,
        string SecurityNote,
        string TeamSignOff );
}
