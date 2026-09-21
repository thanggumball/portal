namespace StudentPortal.Common.DTOs.User;

public class CreateUserRequest
{
    public string FullName { get; init; } = string.Empty;
    public string RoleName { get; init; } = string.Empty;
}
