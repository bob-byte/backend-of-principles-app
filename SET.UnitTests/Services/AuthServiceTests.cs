using BusinessLogic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SET.DataAccess;
using SET.Shared.Helpers;
using SET.Shared.Models;
using SET.Shared.Models.Auth;

namespace SET.UnitTests.Services;

public class AuthServiceTests
{
    private static (AuthService service, AppDbContext db) CreateSut()
    {
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        var config = new Mock<IConfiguration>();
        services.AddSingleton(config.Object);

        var jwt = new Mock<IJwtTokenService>();
        jwt.Setup(j => j.GetToken(It.IsAny<User>())).Returns("test-token");
        services.AddSingleton(jwt.Object);

        ServiceProvider provider = services.BuildServiceProvider();
        var db = provider.GetRequiredService<AppDbContext>();
        var auth = new AuthService(provider);
        return (auth, db);
    }

    [Fact]
    public async Task RegisterAsync_creates_user_with_password_and_default_areas()
    {
        (AuthService auth, AppDbContext db) = CreateSut();

        User user = await auth.RegisterAsync(new UserRegister
        {
            Email = "ada@example.com",
            Name = "Ada",
            Password = "Secret123!",
            Gender = Gender.Woman,
            MainSlogan = "Keep going",
            Mission = "Build tools",
        });

        Assert.True(user.Id != 0);
        Assert.Equal("ada@example.com", user.Email);
        Assert.NotNull(user.Password);
        Assert.True(PasswordHelper.VerifyPasswordHash("Secret123!", user.Password!));

        int areaCount = await db.UserAreasOfLife.CountAsync(a => a.UserId == user.Id);
        Assert.Equal(10, areaCount);
    }

    [Fact]
    public async Task LoginAsync_returns_user_when_password_matches()
    {
        (AuthService auth, AppDbContext db) = CreateSut();
        await auth.RegisterAsync(new UserRegister
        {
            Email = "ada@example.com",
            Name = "Ada",
            Password = "Secret123!",
            Gender = Gender.Woman,
        });

        (User? found, string? error) = await auth.LoginAsync(new UserLogin
        {
            Email = "ADA@example.com",
            Password = "Secret123!",
        });

        Assert.NotNull(found);
        Assert.Null(error);
        Assert.Equal("ada@example.com", found!.Email);
    }

    [Fact]
    public async Task LoginAsync_returns_PasswordIsIncorrect_for_wrong_password()
    {
        (AuthService auth, _) = CreateSut();
        await auth.RegisterAsync(new UserRegister
        {
            Email = "ada@example.com",
            Name = "Ada",
            Password = "Secret123!",
            Gender = Gender.Woman,
        });

        (User? found, string? error) = await auth.LoginAsync(new UserLogin
        {
            Email = "ada@example.com",
            Password = "Nope",
        });

        Assert.NotNull(found);
        Assert.Equal("PasswordIsIncorrect", error);
    }

    [Fact]
    public async Task LoginAsync_returns_EmailIsIncorrect_when_user_missing()
    {
        (AuthService auth, _) = CreateSut();

        (User? found, string? error) = await auth.LoginAsync(new UserLogin
        {
            Email = "missing@example.com",
            Password = "Secret123!",
        });

        Assert.Null(found);
        Assert.Equal("EmailIsIncorrect", error);
    }

    [Fact]
    public async Task LoginAsync_returns_YouDontHavePassword_when_password_null()
    {
        (AuthService auth, AppDbContext db) = CreateSut();
        db.Users.Add(new User
        {
            Email = "oauth@example.com",
            Name = "OAuth",
            Gender = Gender.Other,
            Password = null,
            CreatedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();

        (User? found, string? error) = await auth.LoginAsync(new UserLogin
        {
            Email = "oauth@example.com",
            Password = "anything",
        });

        Assert.NotNull(found);
        Assert.Equal("YouDontHavePassword", error);
    }
}
