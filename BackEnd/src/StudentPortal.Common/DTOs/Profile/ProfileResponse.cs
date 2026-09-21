namespace StudentPortal.Common.DTOs.Profile;

public class ProfileResponse
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string? StudentCode { get; init; }
    public string? AvatarUrl { get; init; }
    public string RoleName { get; init; } = string.Empty;
    public DateTime? LastLoginAt { get; init; }
    public DateTime CreatedAt { get; init; }
}
