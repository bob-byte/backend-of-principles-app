using SET.Shared.Helpers;

namespace SET.UnitTests.Helpers;

public class TextEncryptHelperTests
{
    // AES-128 requires 16-byte key and IV
    private const string FirstKey = "0123456789ABCDEF";
    private const string SecondKey = "FEDCBA9876543210";

    [Fact]
    public void EncryptText_RoundTrip_DecryptsViaPasswordHelper()
    {
        const string plain = "new-password-value";

        string cipher = TextEncryptHelper.EncryptText(plain, FirstKey, SecondKey);
        string decrypted = PasswordHelper.DecryptNewPassword(cipher, FirstKey, SecondKey);

        Assert.False(string.IsNullOrWhiteSpace(cipher));
        Assert.NotEqual(plain, cipher);
        Assert.Equal(plain, decrypted);
    }

    [Fact]
    public void EncryptText_Plaintext_ProducesDifferentCiphertext()
    {
        string cipher = TextEncryptHelper.EncryptText("hello", FirstKey, SecondKey);

        Assert.NotEqual("hello", cipher);
        Assert.True(Convert.FromBase64String(cipher).Length > 0);
    }
}
