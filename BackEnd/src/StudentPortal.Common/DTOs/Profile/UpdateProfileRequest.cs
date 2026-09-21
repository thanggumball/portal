namespace StudentPortal.Common.DTOs.Profile;

public class UpdateProfileRequest
{
    public string FullName { get; init; } = string.Empty;
    public string? AvatarUrl { get; init; }
}
