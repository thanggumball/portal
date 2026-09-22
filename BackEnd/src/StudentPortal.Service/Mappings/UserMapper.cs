using StudentPortal.Repository.Entities;
using StudentPortal.Common.DTOs.User;

namespace StudentPortal.Service.Mappers;

public static class UserMapper
{
    public static UserResponse ToResponse(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            UserName = user.UserName,
            FullName = user.FullName,
            UserCode = user.UserCode,
            RoleName = user.Role.Name,
            Status = user.Status,
            AvatarUrl = user.AvatarUrl,
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt
        };
    }
}
