using BusinessLogic;
using BusinessLogic.Models;
using Moq;
using SET.DataAccess;
using SET.Shared.Models;
using SET.UnitTests.TestSupport;

namespace SET.UnitTests.Services;

public class RefreshTokenServiceTests
{
    private readonly AppDbContext m_db = TestDb.Create();
    private readonly Mock<IJwtTokenService> m_jwt = new();

    private RefreshTokenService CreateSut() => new( m_db, m_jwt.Object );

    [Fact]
    public async Task IssueAsync_PersistsHashAndReturnsPlaintext()
    {
        await TestData.AddUserAsync( m_db, 7 );

        string plaintext = await CreateSut().IssueAsync( 7, "device-1" );

        Assert.False( string.IsNullOrWhiteSpace( plaintext ) );
        RefreshToken row = Assert.Single( m_db.RefreshTokens );
        Assert.Equal( 7, row.UserId );
        Assert.Equal( "device-1", row.DeviceId );
        Assert.Equal( RefreshTokenService.Hash( plaintext ), row.TokenHash );
        Assert.Null( row.RevokedAt );
        Assert.True( row.ExpiresAt > DateTime.UtcNow.AddDays( 100 ) );
    }

    [Fact]
    public async Task RotateAsync_ValidToken_ReturnsNewPairAndRevokesOld()
    {
        User user = await TestData.AddUserAsync( m_db, 3 );
        RefreshTokenService sut = CreateSut();
        string oldPlain = await sut.IssueAsync( 3 );
        m_jwt.Setup( j => j.GetToken( It.Is<User>( u => u.Id == 3 ) ) ).Returns( "access-2" );

        ServiceResult<LoginResponse> result = await sut.RotateAsync( oldPlain, "device-2" );

        Assert.True( result.IsSuccess );
        Assert.Equal( "access-2", result.Value!.Token );
        Assert.False( string.IsNullOrWhiteSpace( result.Value.RefreshToken ) );
        Assert.NotEqual( oldPlain, result.Value.RefreshToken );

        List<RefreshToken> rows = m_db.RefreshTokens.OrderBy( t => t.Id ).ToList();
        Assert.Equal( 2, rows.Count );
        Assert.NotNull( rows[0].RevokedAt );
        Assert.Equal( RefreshTokenService.Hash( result.Value.RefreshToken! ), rows[0].ReplacedByTokenHash );
        Assert.Null( rows[1].RevokedAt );
        Assert.Equal( "device-2", rows[1].DeviceId );
        Assert.Equal( user.Id, rows[1].UserId );
    }

    [Fact]
    public async Task RotateAsync_ReusedRevokedToken_RevokesAllAndFails()
    {
        await TestData.AddUserAsync( m_db, 5 );
        RefreshTokenService sut = CreateSut();
        string first = await sut.IssueAsync( 5 );
        m_jwt.Setup( j => j.GetToken( It.IsAny<User>() ) ).Returns( "access" );
        ServiceResult<LoginResponse> rotated = await sut.RotateAsync( first );
        Assert.True( rotated.IsSuccess );

        await sut.IssueAsync( 5 );
        // The replacement is in use, so presenting the old token is not a retry.
        Assert.True( ( await sut.RotateAsync( rotated.Value!.RefreshToken! ) ).IsSuccess );

        ServiceResult<LoginResponse> reuse = await sut.RotateAsync( first );

        TestData.AssertError( reuse, 400, "RefreshTokenReuseDetected" );
        Assert.All( m_db.RefreshTokens.Where( t => t.UserId == 5 ), t => Assert.NotNull( t.RevokedAt ) );
    }

    [Fact]
    public async Task RotateAsync_RetryAfterLostResponse_IssuesNewPair()
    {
        await TestData.AddUserAsync( m_db, 5 );
        RefreshTokenService sut = CreateSut();
        string first = await sut.IssueAsync( 5 );
        m_jwt.Setup( j => j.GetToken( It.IsAny<User>() ) ).Returns( "access" );
        string lost = ( await sut.RotateAsync( first ) ).Value!.RefreshToken!;

        ServiceResult<LoginResponse> retry = await sut.RotateAsync( first );
        ServiceResult<LoginResponse> secondRetry = await sut.RotateAsync( first );

        Assert.True( retry.IsSuccess );
        Assert.True( secondRetry.IsSuccess );
        RefreshToken active = Assert.Single( m_db.RefreshTokens.Where( t => t.RevokedAt == null ) );
        Assert.Equal( RefreshTokenService.Hash( secondRetry.Value!.RefreshToken! ), active.TokenHash );
        Assert.NotNull( m_db.RefreshTokens.Single( t => t.TokenHash == RefreshTokenService.Hash( lost ) ).RevokedAt );
    }

    [Fact]
    public async Task RotateAsync_RetryAfterWindow_RevokesAllAndFails()
    {
        await TestData.AddUserAsync( m_db, 5 );
        RefreshTokenService sut = CreateSut();
        string first = await sut.IssueAsync( 5 );
        m_jwt.Setup( j => j.GetToken( It.IsAny<User>() ) ).Returns( "access" );
        await sut.RotateAsync( first );
        RefreshToken rotated = m_db.RefreshTokens.Single( t => t.RevokedAt != null );
        rotated.RevokedAt = DateTime.UtcNow - RefreshTokenService.RotationRetryWindow - TimeSpan.FromSeconds( 1 );
        await m_db.SaveChangesAsync();

        ServiceResult<LoginResponse> reuse = await sut.RotateAsync( first );

        TestData.AssertError( reuse, 400, "RefreshTokenReuseDetected" );
        Assert.All( m_db.RefreshTokens, t => Assert.NotNull( t.RevokedAt ) );
    }

    [Fact]
    public async Task RotateAsync_TokenEndedByLogoutOrPasswordChange_KeepsOtherSessions()
    {
        await TestData.AddUserAsync( m_db, 5 );
        RefreshTokenService sut = CreateSut();
        string stale = await sut.IssueAsync( 5 );
        await sut.RevokeAllForUserAsync( 5 );
        string current = await sut.IssueAsync( 5 );

        TestData.AssertError( await sut.RotateAsync( stale ), 400, "InvalidRefreshToken" );

        RefreshToken active = m_db.RefreshTokens.Single( t => t.TokenHash == RefreshTokenService.Hash( current ) );
        Assert.Null( active.RevokedAt );
    }

    [Fact]
    public async Task RotateAsync_Expired_ReturnsBadRequest()
    {
        await TestData.AddUserAsync( m_db, 5 );
        RefreshTokenService sut = CreateSut();
        string plain = await sut.IssueAsync( 5 );
        m_db.RefreshTokens.Single().ExpiresAt = DateTime.UtcNow.AddMinutes( -1 );
        await m_db.SaveChangesAsync();

        TestData.AssertError( await sut.RotateAsync( plain ), 400, "RefreshTokenExpired" );
    }

    [Fact]
    public async Task RotateAsync_UnknownOrBlank_ReturnsBadRequest()
    {
        RefreshTokenService sut = CreateSut();
        TestData.AssertError( await sut.RotateAsync( " " ), 400, "RefreshTokenIsRequired" );
        TestData.AssertError( await sut.RotateAsync( "missing" ), 400, "InvalidRefreshToken" );
    }
}
