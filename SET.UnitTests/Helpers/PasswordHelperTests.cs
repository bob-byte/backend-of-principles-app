using SET.Shared.Helpers;

namespace SET.UnitTests.Helpers;

public class PasswordHelperTests
{
    [Fact]
    public void CreatePasswordHash_returns_192_byte_hash_and_salt()
    {
        byte[] stored = PasswordHelper.CreatePasswordHash("Secret123!");

        Assert.Equal(192, stored.Length);
        Assert.True(PasswordHelper.VerifyPasswordHash("Secret123!", stored));
    }

    [Fact]
    public void VerifyPasswordHash_returns_false_for_wrong_password()
    {
        byte[] stored = PasswordHelper.CreatePasswordHash("Secret123!");

        Assert.False(PasswordHelper.VerifyPasswordHash("WrongPassword", stored));
    }

    [Fact]
    public void CreatePasswordHash_throws_for_null()
    {
        Assert.Throws<ArgumentNullException>(() => PasswordHelper.CreatePasswordHash(null!));
    }

    [Fact]
    public void CreatePasswordHash_throws_for_whitespace()
    {
        Assert.Throws<ArgumentException>(() => PasswordHelper.CreatePasswordHash("   "));
    }

    [Fact]
    public void VerifyPasswordHash_throws_for_invalid_length()
    {
        Assert.Throws<ArgumentException>(() =>
            PasswordHelper.VerifyPasswordHash("Secret123!", new byte[10]));
    }

    [Fact]
    public void VerifyPasswordHash_throws_for_null_password()
    {
        byte[] stored = PasswordHelper.CreatePasswordHash("Secret123!");
        Assert.Throws<ArgumentNullException>(() =>
            PasswordHelper.VerifyPasswordHash(null!, stored));
    }
}
