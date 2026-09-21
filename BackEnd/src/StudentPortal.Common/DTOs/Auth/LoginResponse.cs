namespace StudentPortal.Common.DTOs.Auth;

// Shared by both login and refresh token - both return a new token pair.
public class LoginResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
}
