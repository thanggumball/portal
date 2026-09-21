namespace StudentPortal.Common.DTOs.Auth;

public class LogoutRequest
{
    public string RefreshToken { get; init; } = string.Empty;
}
