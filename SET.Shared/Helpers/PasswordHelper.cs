using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;

using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SET.Shared.Helpers;

public static class PasswordHelper
{
    public static byte[] CreatePasswordHash( string password )
    {
        if (password is null)
        {
            throw new ArgumentNullException(nameof(password));
        }

        if (string.IsNullOrWhiteSpace(password))
        { 
            throw new ArgumentException("Value cannot be empty or whitespace only string.", nameof(password));
        }

        using HMACSHA512 hmac = new();

        //length is 128
        byte[] passwordSalt = hmac.Key;

        //length is 64
        byte[] passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

        return passwordHash.Concat(passwordSalt).ToArray();
    }

    public static bool VerifyPasswordHash(string password, byte[] storedPasswordHash)
    {
        if (password == null)
        {
            throw new ArgumentNullException(nameof(password));
        }

        if (storedPasswordHash?.Length != 192)
        {
            throw new ArgumentException("Invalid length of password hash", nameof(storedPasswordHash));
        }

        byte[] storedSalt = storedPasswordHash[64..];
        byte[] storedHash = storedPasswordHash[..64];

        using var hmac = new HMACSHA512(storedSalt);

        byte[] computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

        return computedHash.SequenceEqual(storedHash);
    }

    public static string DecryptNewPassword( string cipherText, string firstKey, string secondKey )
    {
        byte[] Key = Encoding.UTF8.GetBytes( firstKey );

        byte[] IV = Encoding.UTF8.GetBytes( secondKey );

        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Key;
            aesAlg.IV = IV;

            ICryptoTransform decryptor = aesAlg.CreateDecryptor( aesAlg.Key, aesAlg.IV );

            using (MemoryStream msDecrypt = new MemoryStream( Convert.FromBase64String( cipherText ) ))
            using (CryptoStream csDecrypt = new CryptoStream( msDecrypt, decryptor, CryptoStreamMode.Read ))
            using (StreamReader srDecrypt = new StreamReader( csDecrypt ))
            {
                return srDecrypt.ReadToEnd();
            }
        }
    }
}

