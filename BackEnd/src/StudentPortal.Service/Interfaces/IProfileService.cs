using StudentPortal.Common.DTOs.Profile;

namespace StudentPortal.Service.Interfaces;

public interface IProfileService
{
    Task<ProfileResponse> GetMyProfileAsync(Guid userId, CancellationToken ct = default);
    Task<ProfileResponse> UpdateMyProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken ct = default);
}
