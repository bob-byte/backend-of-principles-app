using BusinessLogic;
using BusinessLogic.Models;
using Microsoft.Extensions.Configuration;
using Moq;
using SET.DataAccess;
using SET.Shared.Helpers;
using SET.Shared.Models;
using SET.Shared.Models.Auth;
using SET.UnitTests.TestSupport;

namespace SET.UnitTests.Services;

public class AccountServiceTests
{
    private const string FirstKey = "0123456789abcdef0123456789abcdef";
    private const string SecondKey = "abcdef9876543210";

    private readonly AppDbContext m_db = TestDb.Create();
    private readonly Mock<IAuthService> m_auth = new( MockBehavior.Strict );
    private readonly Mock<IJwtTokenService> m_jwt = new();
    private readonly Mock<IEmailSender> m_email = new();
    private readonly IConfiguration m_configuration = new ConfigurationBuilder()
        .AddInMemoryCollection( new Dictionary<string, string?>
        {
            ["EncryptionSettings:FirstKey"] = FirstKey,
            ["EncryptionSettings:SecondKey"] = SecondKey,
            ["AI_API_KEY"] = "secret-ai-key",
            ["EncryptionSettingsForAiApi:FirstKey"] = FirstKey,
            ["EncryptionSettingsForAiApi:SecondKey"] = SecondKey,
        } )
        .Build();

    private AccountService CreateSut() => new( m_db, m_auth.Object, m_jwt.Object, m_email.Object, m_configuration );

    private static string Encrypt( string plain ) => TextEncryptHelper.EncryptText( plain, FirstKey, SecondKey );

    [Fact]
    public async Task FindUserAsync_returns_user_or_null()
    {
        await TestData.AddUserAsync( m_db, 3 );

        Assert.Equal( 3, (await CreateSut().FindUserAsync( 3 ))!.Id );
        Assert.Null( await CreateSut().FindUserAsync( 4 ) );
    }

    [Fact]
    public async Task GoogleAuthAsync_validates_tokens_then_delegates()
    {
        AccountService service = CreateSut();
        TestData.AssertError( await service.GoogleAuthAsync( null! ), 400, "RequestBodyIsNull" );
        TestData.AssertError( await service.GoogleAuthAsync( new GoogleLoginRequest { IdToken = "id" } ), 400, "AccessTokenIsNull" );
        TestData.AssertError( await service.GoogleAuthAsync( new GoogleLoginRequest { AccessToken = "a" } ), 400, "IdTokenIsNull" );

        m_auth.Setup( a => a.GoogleAuthAsync( "id", "a" ) ).ReturnsAsync( new GoogleAuthResponse( "jwt" ) );

        ServiceResult<GoogleAuthResponse> result =
            await service.GoogleAuthAsync( new GoogleLoginRequest { IdToken = "id", AccessToken = "a" } );

        Assert.Equal( "jwt", result.Value!.Token );
    }

    [Fact]
    public async Task RegisterAsync_validates_required_fields()
    {
        AccountService service = CreateSut();

        TestData.AssertError( await service.RegisterAsync( null! ), 400, "RegisterInfoIsNull" );
        TestData.AssertError( await service.RegisterAsync( new UserRegister { Password = "x" } ), 400, "EmailOfRegisterInfoIsNull" );
        TestData.AssertError( await service.RegisterAsync( new UserRegister { Email = "a@b.c" } ), 400, "PasswordShouldBeFilled" );
    }

    [Fact]
    public async Task RegisterAsync_rejects_existing_email_case_insensitively()
    {
        await TestData.AddUserAsync( m_db, 1, "Taken@Example.com" );

        ServiceResult result = await CreateSut().RegisterAsync( new UserRegister
        {
            Email = "taken@example.com",
            Password = Encrypt( "password1" ),
        } );

        TestData.AssertError( result, 400, "UserWithIdenticalEmailAlreadyExists" );
    }

    [Theory]
    [InlineData( "short" )]
    [InlineData( "this-password-is-way-too-long" )]
    public async Task RegisterAsync_rejects_password_length( string password )
    {
        ServiceResult result = await CreateSut().RegisterAsync( new UserRegister
        {
            Email = "new@example.com",
            Password = Encrypt( password ),
        } );

        TestData.AssertError( result, 400, "PasswordLengthIsLessThanMinOrMoreThanMaxCharacters" );
    }

    [Fact]
    public async Task RegisterAsync_rejects_undecryptable_password()
    {
        ServiceResult result = await CreateSut().RegisterAsync( new UserRegister
        {
            Email = "new@example.com",
            Password = "not base64!",
        } );

        TestData.AssertError( result, 400, "InvalidPassword" );
    }

    [Fact]
    public async Task RegisterAsync_passes_decrypted_password_and_default_name()
    {
        UserRegister? registered = null;
        m_auth.Setup( a => a.RegisterAsync( It.IsAny<UserRegister>() ) )
            .Callback<UserRegister>( r => registered = r )
            .ReturnsAsync( new User() );

        ServiceResult result = await CreateSut().RegisterAsync( new UserRegister
        {
            Email = "new@example.com",
            Password = Encrypt( "password1" ),
            Name = null!,
        } );

        Assert.True( result.IsSuccess );
        Assert.Equal( "password1", registered!.Password );
        Assert.Equal( string.Empty, registered.Name );
    }

    [Fact]
    public async Task LoginAsync_validates_required_fields()
    {
        AccountService service = CreateSut();

        TestData.AssertError( await service.LoginAsync( null! ), 400, "UserLoginRequestObjectIsNull" );
        TestData.AssertError( await service.LoginAsync( new UserLogin { Password = "x" } ), 400, "EmailShouldBeFilled" );
        TestData.AssertError( await service.LoginAsync( new UserLogin { Email = "a@b.c" } ), 400, "PasswordShouldBeFilled" );
        TestData.AssertError( await service.LoginAsync( new UserLogin { Email = "a@b.c", Password = "garbage" } ), 400, "InvalidEmailOrPassword" );
    }

    [Fact]
    public async Task LoginAsync_returns_token_for_valid_credentials()
    {
        User user = new() { Id = 9 };
        m_auth.Setup( a => a.LoginAsync( It.Is<UserLogin>( l => l.Password == "password1" ) ) )
            .ReturnsAsync( (user, (string?)null) );
        m_jwt.Setup( j => j.GetToken( user ) ).Returns( "jwt-9" );

        ServiceResult<LoginResponse> result =
            await CreateSut().LoginAsync( new UserLogin { Email = "a@b.c", Password = Encrypt( "password1" ) } );

        Assert.Equal( "jwt-9", result.Value!.Token );
    }

    [Theory]
    [InlineData( "EmailIsIncorrect", "InvalidEmailOrPassword" )]
    [InlineData( "PasswordIsIncorrect", "InvalidEmailOrPassword" )]
    [InlineData( "UserIsBlocked", "UserIsBlocked" )]
    public async Task LoginAsync_masks_credential_errors( string authError, string expected )
    {
        m_auth.Setup( a => a.LoginAsync( It.IsAny<UserLogin>() ) ).ReturnsAsync( ((User?)null, authError) );

        ServiceResult<LoginResponse> result =
            await CreateSut().LoginAsync( new UserLogin { Email = "a@b.c", Password = Encrypt( "password1" ) } );

        TestData.AssertError( result, 400, expected );
    }

    [Fact]
    public async Task SendPasswordResetCodeAsync_requires_registered_email_and_sends_code()
    {
        await TestData.AddUserAsync( m_db, 1, "Me@Example.com" );
        AccountService service = CreateSut();

        TestData.AssertError( await service.SendPasswordResetCodeAsync( "nobody@example.com", "en" ), 400, "EmailIsIncorrect" );

        ServiceResult<GenerateCodeResponse> result = await service.SendPasswordResetCodeAsync( "me@example.com", "en" );

        Assert.InRange( result.Value!.Code, 100000, 999998 );
        m_email.Verify( e => e.SendAsync(
            "me@example.com",
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>() ), Times.Once );
    }

    [Fact]
    public async Task SendSignupCodeAsync_rejects_blank_and_registered_emails()
    {
        await TestData.AddUserAsync( m_db, 1, "me@example.com" );
        AccountService service = CreateSut();

        TestData.AssertError( await service.SendSignupCodeAsync( " ", null ), 400, "EmailWhereSendCodeIsNullOrWhiteSpace" );
        TestData.AssertError( await service.SendSignupCodeAsync( "ME@example.com", null ), 400, "UserWithIdenticalEmailAlreadyExists" );
        m_email.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SendSignupCodeAsync_emails_code_to_new_address()
    {
        ServiceResult<GenerateCodeResponse> result = await CreateSut().SendSignupCodeAsync( "new@example.com", "uk" );

        Assert.InRange( result.Value!.Code, 100000, 999998 );
        m_email.Verify( e => e.SendAsync(
            "new@example.com",
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>() ), Times.Once );
    }

    [Fact]
    public void GetEncryptedAiApiKey_encrypts_configured_key()
    {
        EncryptedValueResponse response = CreateSut().GetEncryptedAiApiKey();

        Assert.Equal( "secret-ai-key", PasswordHelper.DecryptNewPassword( response.Value, FirstKey, SecondKey ) );
    }

    [Fact]
    public async Task ChangePasswordAsync_validates_request()
    {
        await TestData.AddUserAsync( m_db, 1, "me@example.com" );
        AccountService service = CreateSut();

        TestData.AssertError( await service.ChangePasswordAsync( null! ), 400, "Request is null" );
        TestData.AssertError( await service.ChangePasswordAsync( new UserNewPassword { NewPassword = "x" } ), 400, "EmailIsNullOrWhiteSpace" );
        TestData.AssertError( await service.ChangePasswordAsync( new UserNewPassword { Email = "me@example.com" } ), 400, "PasswordIsNullOrWhiteSpace" );
        TestData.AssertError(
            await service.ChangePasswordAsync( new UserNewPassword { Email = "other@example.com", NewPassword = Encrypt( "password1" ) } ),
            400,
            "EmailIsIncorrect" );
        TestData.AssertError(
            await service.ChangePasswordAsync( new UserNewPassword { Email = "me@example.com", NewPassword = Encrypt( "short" ) } ),
            400,
            "PasswordLengthIsLessThanMinOrMoreThanMaxCharacters" );
        TestData.AssertError(
            await service.ChangePasswordAsync( new UserNewPassword { Email = "me@example.com", NewPassword = "garbage" } ),
            400,
            "IncorrectPassword" );
    }

    [Fact]
    public async Task ChangePasswordAsync_stores_new_hash()
    {
        User user = await TestData.AddUserAsync( m_db, 1, "me@example.com" );

        ServiceResult result = await CreateSut().ChangePasswordAsync( new UserNewPassword
        {
            Email = "ME@example.com",
            NewPassword = Encrypt( "password1" ),
        } );

        Assert.True( result.IsSuccess );
        Assert.NotNull( user.Password );
        Assert.Equal( 64 + 128, user.Password!.Length );
    }

    [Fact]
    public async Task Missing_password_encryption_keys_fail_loudly()
    {
        AccountService service = new( m_db, m_auth.Object, m_jwt.Object, m_email.Object, new ConfigurationBuilder().Build() );

        await Assert.ThrowsAsync<InvalidProgramException>( () =>
            service.LoginAsync( new UserLogin { Email = "a@b.c", Password = "x" } ) );
    }
}
