using StudentPortal.Common.DTOs.Profile;
using StudentPortal.Common.Exceptions;
using StudentPortal.Repository.Interfaces;
using StudentPortal.Service.Interfaces;
using StudentPortal.Service.Mappers;

namespace StudentPortal.Service.Implementations;

public class ProfileService : IProfileService
{
    private readonly IProfileRepository _profileRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProfileService(
        IProfileRepository profileRepository,
        IUnitOfWork unitOfWork)
    {
        _profileRepository = profileRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProfileResponse> GetMyProfileAsync(
        Guid userId,
        CancellationToken ct = default)
    {
        var user = await _profileRepository.GetMyProfileAsync(userId, ct);

        if (user is null || user.IsDeleted)
        {
            throw new NotFoundException("User not found.");
        }

        return ProfileMapper.ToResponse(user);
    }

    //public async Task<ProfileResponse> UpdateMyProfileAsync(
    //    Guid userId,
    //    UpdateProfileRequest request,
    //    CancellationToken ct = default)
    //{
    //    var user = await _profileRepository.GetByIdAsync(userId, ct);

    //    if (user is null || user.IsDeleted)
    //    {
    //        throw new NotFoundException("User not found.");
    //    }

    //    if (string.IsNullOrWhiteSpace(request.FullName))
    //    {
    //        throw new BadRequestException("Full name is required.");
    //    }

    //    user.FullName = request.FullName.Trim();
    //    user.AvatarUrl = request.AvatarUrl?.Trim();
    //    user.UpdatedAt = DateTime.UtcNow;

    //    _userRepository.Update(user);

    //    await _unitOfWork.SaveChangesAsync(ct);

    //    return ProfileMapper.ToResponse(user);
    //}
}
