using StudentPortal.Common.Enums;

namespace StudentPortal.Common.DTOs.User;

public class UserResponse
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string? UserCode { get; init; }
    public string RoleName { get; init; } = string.Empty;
    public UserStatus Status { get; init; }
    public string? AvatarUrl { get; init; }
    public DateTime? LastLoginAt { get; init; }
    public DateTime CreatedAt { get; init; }
}
