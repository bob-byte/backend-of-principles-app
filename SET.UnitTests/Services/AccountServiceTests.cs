using BusinessLogic;
using BusinessLogic.Models;
using Microsoft.EntityFrameworkCore;
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
        } )
        .Build();

    private AccountService CreateSut() => new( m_db, m_auth.Object, m_jwt.Object, m_email.Object, m_configuration );

    private static string Encrypt( string plain ) => TextEncryptHelper.EncryptText( plain, FirstKey, SecondKey );

    private async Task<int> SeedVerificationCodeAsync( string email, string purpose )
    {
        int code = 123456;
        m_db.EmailVerificationCodes.Add( new EmailVerificationCode
        {
            Email = email.Trim().ToLowerInvariant(),
            Purpose = purpose,
            CodeHash = Convert.ToHexString(
                System.Security.Cryptography.SHA256.HashData(
                    System.Text.Encoding.UTF8.GetBytes( code.ToString( "D6" ) ) ) ),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes( 15 ),
        } );
        await m_db.SaveChangesAsync();
        return code;
    }

    [Fact]
    public async Task FindUserAsync_ExistingAndMissingIds_ReturnsUserOrNull()
    {
        await TestData.AddUserAsync( m_db, 3 );

        Assert.Equal( 3, (await CreateSut().FindUserAsync( 3 ))!.Id );
        Assert.Null( await CreateSut().FindUserAsync( 4 ) );
    }

    [Fact]
    public async Task GoogleAuthAsync_MissingTokens_ReturnsBadRequestThenDelegates()
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
    public async Task RegisterAsync_MissingRequiredFields_ReturnsBadRequest()
    {
        AccountService service = CreateSut();

        TestData.AssertError( await service.RegisterAsync( null! ), 400, "RegisterInfoIsNull" );
        TestData.AssertError( await service.RegisterAsync( new UserRegister { Password = "x" } ), 400, "EmailOfRegisterInfoIsNull" );
        TestData.AssertError( await service.RegisterAsync( new UserRegister { Email = "a@b.c" } ), 400, "PasswordShouldBeFilled" );
        TestData.AssertError(
            await service.RegisterAsync( new UserRegister { Email = "a@b.c", Password = Encrypt( "password1" ) } ),
            400,
            "VerificationCodeIsRequired" );
    }

    [Fact]
    public async Task RegisterAsync_ExistingEmail_ReturnsBadRequest()
    {
        await TestData.AddUserAsync( m_db, 1, "Taken@Example.com" );

        ServiceResult result = await CreateSut().RegisterAsync( new UserRegister
        {
            Email = "taken@example.com",
            Password = Encrypt( "password1" ),
            Code = 123456,
        } );

        TestData.AssertError( result, 400, "UserWithIdenticalEmailAlreadyExists" );
    }

    [Theory]
    [InlineData( "short" )]
    [InlineData( "this-password-is-way-too-long" )]
    public async Task RegisterAsync_InvalidPasswordLength_ReturnsBadRequest( string password )
    {
        await SeedVerificationCodeAsync( "new@example.com", EmailVerificationPurposes.Signup );

        ServiceResult result = await CreateSut().RegisterAsync( new UserRegister
        {
            Email = "new@example.com",
            Password = Encrypt( password ),
            Code = 123456,
        } );

        TestData.AssertError( result, 400, "PasswordLengthIsLessThanMinOrMoreThanMaxCharacters" );
    }

    [Fact]
    public async Task RegisterAsync_UndecryptablePassword_ReturnsBadRequest()
    {
        await SeedVerificationCodeAsync( "new@example.com", EmailVerificationPurposes.Signup );

        ServiceResult result = await CreateSut().RegisterAsync( new UserRegister
        {
            Email = "new@example.com",
            Password = "not base64!",
            Code = 123456,
        } );

        TestData.AssertError( result, 400, "InvalidPassword" );
    }

    [Fact]
    public async Task RegisterAsync_WrongVerificationCode_ReturnsBadRequest()
    {
        await SeedVerificationCodeAsync( "new@example.com", EmailVerificationPurposes.Signup );

        ServiceResult result = await CreateSut().RegisterAsync( new UserRegister
        {
            Email = "new@example.com",
            Password = Encrypt( "password1" ),
            Code = 111111,
        } );

        TestData.AssertError( result, 400, "InvalidVerificationCode" );
    }

    [Fact]
    public async Task RegisterAsync_ValidRequest_DecryptsPasswordAndConsumesCode()
    {
        int code = await SeedVerificationCodeAsync( "new@example.com", EmailVerificationPurposes.Signup );
        UserRegister? registered = null;
        m_auth.Setup( a => a.RegisterAsync( It.IsAny<UserRegister>() ) )
            .Callback<UserRegister>( r => registered = r )
            .ReturnsAsync( new User() );

        ServiceResult result = await CreateSut().RegisterAsync( new UserRegister
        {
            Email = "new@example.com",
            Password = Encrypt( "password1" ),
            Name = null!,
            Code = code,
        } );

        Assert.True( result.IsSuccess );
        Assert.Equal( "password1", registered!.Password );
        Assert.Equal( string.Empty, registered.Name );
        Assert.Empty( m_db.EmailVerificationCodes );
    }

    [Fact]
    public async Task LoginAsync_MissingRequiredFields_ReturnsBadRequest()
    {
        AccountService service = CreateSut();

        TestData.AssertError( await service.LoginAsync( null! ), 400, "UserLoginRequestObjectIsNull" );
        TestData.AssertError( await service.LoginAsync( new UserLogin { Password = "x" } ), 400, "EmailShouldBeFilled" );
        TestData.AssertError( await service.LoginAsync( new UserLogin { Email = "a@b.c" } ), 400, "PasswordShouldBeFilled" );
        TestData.AssertError( await service.LoginAsync( new UserLogin { Email = "a@b.c", Password = "garbage" } ), 400, "InvalidEmailOrPassword" );
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsToken()
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
    public async Task LoginAsync_CredentialErrors_ReturnsMaskedMessage( string authError, string expected )
    {
        m_auth.Setup( a => a.LoginAsync( It.IsAny<UserLogin>() ) ).ReturnsAsync( ((User?)null, authError) );

        ServiceResult<LoginResponse> result =
            await CreateSut().LoginAsync( new UserLogin { Email = "a@b.c", Password = Encrypt( "password1" ) } );

        TestData.AssertError( result, 400, expected );
    }

    [Fact]
    public async Task SendPasswordResetCodeAsync_RegisteredEmail_StoresAndEmailsCode()
    {
        await TestData.AddUserAsync( m_db, 1, "Me@Example.com" );
        AccountService service = CreateSut();

        TestData.AssertError( await service.SendPasswordResetCodeAsync( "nobody@example.com", "en" ), 400, "EmailIsIncorrect" );

        ServiceResult result = await service.SendPasswordResetCodeAsync( "me@example.com", "en" );

        Assert.True( result.IsSuccess );
        Assert.Single( m_db.EmailVerificationCodes.Where( c => c.Purpose == EmailVerificationPurposes.PasswordReset ) );
        m_email.Verify( e => e.SendAsync(
            "me@example.com",
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>() ), Times.Once );
    }

    [Fact]
    public async Task SendSignupCodeAsync_BlankOrRegisteredEmail_ReturnsBadRequest()
    {
        await TestData.AddUserAsync( m_db, 1, "me@example.com" );
        AccountService service = CreateSut();

        TestData.AssertError( await service.SendSignupCodeAsync( " ", null ), 400, "EmailWhereSendCodeIsNullOrWhiteSpace" );
        TestData.AssertError( await service.SendSignupCodeAsync( "ME@example.com", null ), 400, "UserWithIdenticalEmailAlreadyExists" );
        m_email.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SendSignupCodeAsync_NewEmail_StoresAndEmailsCode()
    {
        ServiceResult result = await CreateSut().SendSignupCodeAsync( "new@example.com", "uk" );

        Assert.True( result.IsSuccess );
        Assert.Single( m_db.EmailVerificationCodes );
        m_email.Verify( e => e.SendAsync(
            "new@example.com",
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>() ), Times.Once );
    }

    [Fact]
    public async Task ChangePasswordAsync_InvalidRequest_ReturnsBadRequest()
    {
        await TestData.AddUserAsync( m_db, 1, "me@example.com" );
        AccountService service = CreateSut();

        TestData.AssertError( await service.ChangePasswordAsync( null! ), 400, "Request is null" );
        TestData.AssertError( await service.ChangePasswordAsync( new UserNewPassword { NewPassword = "x" } ), 400, "EmailIsNullOrWhiteSpace" );
        TestData.AssertError( await service.ChangePasswordAsync( new UserNewPassword { Email = "me@example.com" } ), 400, "PasswordIsNullOrWhiteSpace" );
        TestData.AssertError(
            await service.ChangePasswordAsync( new UserNewPassword
            {
                Email = "me@example.com",
                NewPassword = Encrypt( "password1" ),
            } ),
            400,
            "VerificationCodeIsRequired" );
        TestData.AssertError(
            await service.ChangePasswordAsync( new UserNewPassword
            {
                Email = "other@example.com",
                NewPassword = Encrypt( "password1" ),
                Code = 123456,
            } ),
            400,
            "EmailIsIncorrect" );
        TestData.AssertError(
            await service.ChangePasswordAsync( new UserNewPassword
            {
                Email = "me@example.com",
                NewPassword = Encrypt( "short" ),
                Code = 123456,
            } ),
            400,
            "PasswordLengthIsLessThanMinOrMoreThanMaxCharacters" );
        TestData.AssertError(
            await service.ChangePasswordAsync( new UserNewPassword
            {
                Email = "me@example.com",
                NewPassword = "garbage",
                Code = 123456,
            } ),
            400,
            "IncorrectPassword" );
    }

    [Fact]
    public async Task ChangePasswordAsync_ValidCode_StoresNewPasswordHash()
    {
        User user = await TestData.AddUserAsync( m_db, 1, "me@example.com" );
        int code = await SeedVerificationCodeAsync( "me@example.com", EmailVerificationPurposes.PasswordReset );

        ServiceResult wrong = await CreateSut().ChangePasswordAsync( new UserNewPassword
        {
            Email = "ME@example.com",
            NewPassword = Encrypt( "password1" ),
            Code = 111111,
        } );
        TestData.AssertError( wrong, 400, "InvalidVerificationCode" );

        ServiceResult result = await CreateSut().ChangePasswordAsync( new UserNewPassword
        {
            Email = "ME@example.com",
            NewPassword = Encrypt( "password1" ),
            Code = code,
        } );

        Assert.True( result.IsSuccess );
        Assert.NotNull( user.Password );
        Assert.Equal( 64 + 128, user.Password!.Length );
        Assert.Empty( m_db.EmailVerificationCodes );
    }

    [Fact]
    public async Task LoginAsync_MissingEncryptionKeys_ThrowsInvalidProgramException()
    {
        AccountService service = new( m_db, m_auth.Object, m_jwt.Object, m_email.Object, new ConfigurationBuilder().Build() );

        await Assert.ThrowsAsync<InvalidProgramException>( () =>
            service.LoginAsync( new UserLogin { Email = "a@b.c", Password = "x" } ) );
    }
}
