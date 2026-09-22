using StudentPortal.Common.Enums;

namespace StudentPortal.Common.DTOs.User;

public class UpdateUserRequest
{
    public string FullName { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public UserStatus Status { get; set; }
    public string? AvatarUrl { get; set; }
}
