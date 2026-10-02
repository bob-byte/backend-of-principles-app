using BusinessLogic;

namespace SET.UnitTests.Services;

public sealed class VerificationEmailContentTests
{
    [Fact]
    public void Build_signup_includes_purpose_and_formatted_code()
    {
        VerificationEmailContent.Message message =
            VerificationEmailContent.Build( VerificationEmailContent.Purpose.Signup, 123456 );

        Assert.Equal( "Your Principles signup code", message.Subject );
        Assert.Contains( "Confirm your email", message.PlainTextBody );
        Assert.Contains( "123 456", message.PlainTextBody );
        Assert.Contains( "no account will be created", message.PlainTextBody );
        Assert.Contains( "123 456", message.HtmlBody );
        Assert.Contains( "FF6B00", message.HtmlBody );
        Assert.DoesNotContain( "<script", message.HtmlBody );
    }

    [Fact]
    public void Build_password_reset_includes_purpose_and_formatted_code()
    {
        VerificationEmailContent.Message message =
            VerificationEmailContent.Build( VerificationEmailContent.Purpose.PasswordReset, 654321 );

        Assert.Equal( "Your Principles password reset code", message.Subject );
        Assert.Contains( "Reset your password", message.PlainTextBody );
        Assert.Contains( "654 321", message.PlainTextBody );
        Assert.Contains( "password will stay the same", message.PlainTextBody );
        Assert.Contains( "654 321", message.HtmlBody );
        Assert.Contains( "href=\"https://principles.top\"", message.HtmlBody );
    }

    [Fact]
    public void Build_pads_short_codes_to_six_digits()
    {
        VerificationEmailContent.Message message =
            VerificationEmailContent.Build( VerificationEmailContent.Purpose.Signup, 42 );

        Assert.Contains( "000 042", message.PlainTextBody );
        Assert.Contains( "000 042", message.HtmlBody );
    }
}
