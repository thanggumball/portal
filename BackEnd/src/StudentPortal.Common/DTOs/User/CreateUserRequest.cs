namespace StudentPortal.Common.DTOs.User;

public class CreateUserRequest
{
    public string Email { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string? StudentCode { get; init; }
    public Guid RoleId { get; init; }
}
