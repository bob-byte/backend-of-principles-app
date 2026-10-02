using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using BusinessLogic;
using SET.Shared.Models;

namespace SET.UnitTests.Services;

public class JwtTokenServiceTests
{
    private const string Secret = "unit-test-jwt-secret-key-32chars!!";

    [Fact]
    public void GetToken_returns_readable_jwt_with_user_id_and_email()
    {
        var service = new JwtTokenService(() => Secret);
        var user = new User { Id = 142, Email = "ada@example.com", Name = "Ada" };

        string token = service.GetToken(user);

        Assert.False(string.IsNullOrWhiteSpace(token));
        Assert.Equal(142, service.GetUserIdFromJwt(token));

        JwtSecurityToken jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        string? email = jwt.Claims.FirstOrDefault(c =>
            c.Type == ClaimTypes.Email || c.Type == "email")?.Value;
        Assert.Equal("ada@example.com", email);
    }

    [Fact]
    public void GetUserIdFromJwt_throws_for_empty_token()
    {
        var service = new JwtTokenService(() => Secret);

        Assert.Throws<HttpRequestException>(() => service.GetUserIdFromJwt(" "));
    }

    [Fact]
    public void Constructor_throws_when_secret_factory_returns_null()
    {
        Assert.Throws<ArgumentNullException>(() => new JwtTokenService(() => null!));
    }
}
