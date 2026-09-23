using StudentPortal.Common.DTOs.Profile;
using StudentPortal.Repository.Entities;

namespace StudentPortal.Service.Mappers;

public static class ProfileMapper
{
    public static ProfileResponse ToResponse(this User user)
    {
        return new ProfileResponse
        {
            Id = user.Id,
            Email = user.Email,
            UserName = user.UserName,
            FullName = user.FullName,
            StudentCode = user.UserCode,
            AvatarUrl = user.AvatarUrl,
            RoleName = user.Role.Name,
            LastLoginAt = user.LastLoginAt
        };
    }
}
