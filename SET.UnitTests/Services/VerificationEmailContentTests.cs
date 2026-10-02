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
        Assert.Contains( "lang=\"en\"", message.HtmlBody );
        Assert.Contains( "FF6B00", message.HtmlBody );
        Assert.Contains( "Copy code", message.HtmlBody );
        Assert.Contains( "navigator.clipboard.writeText", message.HtmlBody );
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

    [Theory]
    [InlineData( "uk" )]
    [InlineData( "uk-UA" )]
    [InlineData( "UK_ua" )]
    public void Build_signup_uses_ukrainian_copy_when_language_is_uk( string language )
    {
        VerificationEmailContent.Message message =
            VerificationEmailContent.Build(
                VerificationEmailContent.Purpose.Signup,
                123456,
                language );

        Assert.Equal( "Ваш код реєстрації Principles", message.Subject );
        Assert.Contains( "Підтвердіть електронну пошту", message.PlainTextBody );
        Assert.Contains( "обліковий запис не буде створено", message.PlainTextBody );
        Assert.Contains( "lang=\"uk\"", message.HtmlBody );
        Assert.Contains( "Код підтвердження", message.HtmlBody );
        Assert.Contains( "Копіювати код", message.HtmlBody );
    }

    [Fact]
    public void Build_password_reset_uses_ukrainian_copy_when_language_is_uk()
    {
        VerificationEmailContent.Message message =
            VerificationEmailContent.Build(
                VerificationEmailContent.Purpose.PasswordReset,
                654321,
                "uk" );

        Assert.Equal( "Ваш код скидання пароля Principles", message.Subject );
        Assert.Contains( "Скидання пароля", message.PlainTextBody );
        Assert.Contains( "пароль залишиться без змін", message.PlainTextBody );
    }

    [Theory]
    [InlineData( null )]
    [InlineData( "" )]
    [InlineData( "en" )]
    [InlineData( "fr" )]
    public void Build_defaults_to_english_for_unknown_or_missing_language( string? language )
    {
        VerificationEmailContent.Message message =
            VerificationEmailContent.Build(
                VerificationEmailContent.Purpose.Signup,
                111222,
                language );

        Assert.Equal( "Your Principles signup code", message.Subject );
        Assert.Contains( "lang=\"en\"", message.HtmlBody );
    }
}
