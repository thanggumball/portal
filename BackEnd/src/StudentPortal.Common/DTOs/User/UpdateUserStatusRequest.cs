using StudentPortal.Common.Enums;

namespace StudentPortal.Common.DTOs.User;

public class UpdateUserStatusRequest
{
    public UserStatus Status { get; init; }
}
