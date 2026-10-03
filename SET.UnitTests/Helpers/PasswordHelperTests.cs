using SET.Shared.Helpers;

namespace SET.UnitTests.Helpers;

public class PasswordHelperTests
{
    [Fact]
    public void CreatePasswordHash_ValidPassword_Returns192ByteHashAndSalt()
    {
        byte[] stored = PasswordHelper.CreatePasswordHash("Secret123!");

        Assert.Equal(192, stored.Length);
        Assert.True(PasswordHelper.VerifyPasswordHash("Secret123!", stored));
    }

    [Fact]
    public void VerifyPasswordHash_WrongPassword_ReturnsFalse()
    {
        byte[] stored = PasswordHelper.CreatePasswordHash("Secret123!");

        Assert.False(PasswordHelper.VerifyPasswordHash("WrongPassword", stored));
    }

    [Fact]
    public void CreatePasswordHash_NullPassword_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => PasswordHelper.CreatePasswordHash(null!));
    }

    [Fact]
    public void CreatePasswordHash_WhitespacePassword_Throws()
    {
        Assert.Throws<ArgumentException>(() => PasswordHelper.CreatePasswordHash("   "));
    }

    [Fact]
    public void VerifyPasswordHash_InvalidLength_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            PasswordHelper.VerifyPasswordHash("Secret123!", new byte[10]));
    }

    [Fact]
    public void VerifyPasswordHash_NullPassword_Throws()
    {
        byte[] stored = PasswordHelper.CreatePasswordHash("Secret123!");
        Assert.Throws<ArgumentNullException>(() =>
            PasswordHelper.VerifyPasswordHash(null!, stored));
    }
}
