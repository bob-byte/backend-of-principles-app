using System;
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

        byte[] passwordSalt = new byte[128];
        byte[] passwordHash = new byte[ 64 ];

        using ( var hmac = new HMACSHA512())
        {
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        return passwordHash.Concat(passwordSalt).ToArray();
    }

    public static bool VerifyPasswordHash(string password, byte[] storedPasswordHash)
    {
        if (password == null)
        {
            throw new ArgumentNullException(nameof(password));
        }

        if (storedPasswordHash.Length != 192)
        {
            throw new ArgumentException("Invalid length of password hash", nameof(storedPasswordHash));
        }

        byte[] storedSalt = storedPasswordHash[64..];
        byte[] storedHash = storedPasswordHash[..64];

        using var hmac = new HMACSHA512(storedSalt);

        byte[] computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

        return computedHash.SequenceEqual(storedHash);
    }
}

