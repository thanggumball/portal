namespace StudentPortal.Common.DTOs.User;

public class UpdateUserRequest
{
    public string FullName { get; init; } = string.Empty;
    public string? StudentCode { get; init; }
    public Guid RoleId { get; init; }
}
