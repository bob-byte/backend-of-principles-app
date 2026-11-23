using System.Security.Cryptography;
using System.Text;

using Microsoft.EntityFrameworkCore;

using SET.Shared.Models;

namespace BusinessLogic;

public class RefreshTokenService : IRefreshTokenService
{
    /// <summary>How long a refresh token stays valid without use.</summary>
    public static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays( 180 );

    /// <summary>
    /// How long a rotated token may be retried when its replacement was never used,
    /// i.e. the client lost the response.
    /// </summary>
    public static readonly TimeSpan RotationRetryWindow = TimeSpan.FromSeconds( 60 );

    private readonly AppDbContext m_dbContext;
    private readonly IJwtTokenService m_jwtTokenService;

    public RefreshTokenService( AppDbContext dbContext, IJwtTokenService jwtTokenService )
    {
        m_dbContext = dbContext;
        m_jwtTokenService = jwtTokenService;
    }

    public async Task<string> IssueAsync( long userId, string? deviceId = null )
    {
        string plaintext = CreateSecureToken();
        DateTime now = DateTime.UtcNow;
        m_dbContext.RefreshTokens.Add( new RefreshToken
        {
            UserId = userId,
            TokenHash = Hash( plaintext ),
            CreatedAt = now,
            ExpiresAt = now.Add( RefreshTokenLifetime ),
            DeviceId = NormalizeDeviceId( deviceId ),
        } );
        await m_dbContext.SaveChangesAsync().DefaultConfigureAwait();
        return plaintext;
    }

    public async Task<ServiceResult<LoginResponse>> RotateAsync(
        string refreshTokenPlain,
        string? deviceId = null )
    {
        if (string.IsNullOrWhiteSpace( refreshTokenPlain ))
        {
            return ServiceError.BadRequest( "RefreshTokenIsRequired" );
        }

        string hash = Hash( refreshTokenPlain.Trim() );
        RefreshToken? existing = await m_dbContext.RefreshTokens
            .Include( t => t.User )
            .FirstOrDefaultAsync( t => t.TokenHash == hash )
            .DefaultConfigureAwait();

        if (existing is null)
        {
            return ServiceError.BadRequest( "InvalidRefreshToken" );
        }

        if (existing.RevokedAt is not null)
        {
            if (existing.ReplacedByTokenHash is null)
            {
                // Ended by logout or a password change, not rotated: an ordinary stale session.
                return ServiceError.BadRequest( "InvalidRefreshToken" );
            }

            RefreshToken? successor = await FindRetryableSuccessorAsync( existing ).DefaultConfigureAwait();
            if (successor is null)
            {
                // Possible theft: revoke every session of this user.
                await RevokeAllForUserAsync( existing.UserId ).DefaultConfigureAwait();
                return ServiceError.BadRequest( "RefreshTokenReuseDetected" );
            }

            return await ReplaceAsync( existing, successor, deviceId ).DefaultConfigureAwait();
        }

        if (existing.ExpiresAt < DateTime.UtcNow)
        {
            existing.RevokedAt = DateTime.UtcNow;
            await m_dbContext.SaveChangesAsync().DefaultConfigureAwait();
            return ServiceError.BadRequest( "RefreshTokenExpired" );
        }

        return await ReplaceAsync( existing, existing, deviceId ).DefaultConfigureAwait();
    }

    private async Task<RefreshToken?> FindRetryableSuccessorAsync( RefreshToken rotated )
    {
        if (rotated.RevokedAt < DateTime.UtcNow.Subtract( RotationRetryWindow ))
        {
            return null;
        }

        return await m_dbContext.RefreshTokens
            .FirstOrDefaultAsync( t => t.TokenHash == rotated.ReplacedByTokenHash && t.RevokedAt == null )
            .DefaultConfigureAwait();
    }

    /// <summary>Revokes <paramref name="active"/> and issues its replacement.</summary>
    /// <param name="presented">
    /// The token the client sent; same as <paramref name="active"/> unless it is a retry.
    /// </param>
    private async Task<LoginResponse> ReplaceAsync(
        RefreshToken presented,
        RefreshToken active,
        string? deviceId )
    {
        User user = presented.User;
        string newPlaintext = CreateSecureToken();
        string newHash = Hash( newPlaintext );
        DateTime now = DateTime.UtcNow;

        active.RevokedAt = now;
        active.ReplacedByTokenHash = newHash;
        presented.ReplacedByTokenHash = newHash;

        m_dbContext.RefreshTokens.Add( new RefreshToken
        {
            UserId = user.Id,
            TokenHash = newHash,
            CreatedAt = now,
            ExpiresAt = now.Add( RefreshTokenLifetime ),
            DeviceId = NormalizeDeviceId( deviceId ) ?? active.DeviceId,
        } );

        await m_dbContext.SaveChangesAsync().DefaultConfigureAwait();

        return new LoginResponse(
            Token: m_jwtTokenService.GetToken( user ),
            RefreshToken: newPlaintext );
    }

    public async Task RevokeAsync( string refreshTokenPlain )
    {
        if (string.IsNullOrWhiteSpace( refreshTokenPlain ))
        {
            return;
        }

        string hash = Hash( refreshTokenPlain.Trim() );
        RefreshToken? existing = await m_dbContext.RefreshTokens
            .FirstOrDefaultAsync( t => t.TokenHash == hash )
            .DefaultConfigureAwait();
        if (existing is null || existing.RevokedAt is not null)
        {
            return;
        }

        existing.RevokedAt = DateTime.UtcNow;
        await m_dbContext.SaveChangesAsync().DefaultConfigureAwait();
    }

    public async Task RevokeAllForUserAsync( long userId )
    {
        DateTime now = DateTime.UtcNow;
        List<RefreshToken> active = await m_dbContext.RefreshTokens
            .Where( t => t.UserId == userId && t.RevokedAt == null )
            .ToListAsync()
            .DefaultConfigureAwait();
        foreach (RefreshToken token in active)
        {
            token.RevokedAt = now;
        }

        if (active.Count > 0)
        {
            await m_dbContext.SaveChangesAsync().DefaultConfigureAwait();
        }
    }

    internal static string Hash( string plaintext )
    {
        byte[] bytes = SHA256.HashData( Encoding.UTF8.GetBytes( plaintext ) );
        return Convert.ToHexString( bytes );
    }

    private static string CreateSecureToken()
    {
        byte[] bytes = RandomNumberGenerator.GetBytes( 32 );
        return Convert.ToBase64String( bytes )
            .TrimEnd( '=' )
            .Replace( '+', '-' )
            .Replace( '/', '_' );
    }

    private static string? NormalizeDeviceId( string? deviceId )
    {
        string? value = deviceId?.Trim();
        return string.IsNullOrEmpty( value ) || value.Length > 64 ? null : value;
    }
}
