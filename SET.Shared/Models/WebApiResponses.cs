namespace SET.Shared.Models;

public record GoogleAuthResponse( string Token, string? RefreshToken = null );
