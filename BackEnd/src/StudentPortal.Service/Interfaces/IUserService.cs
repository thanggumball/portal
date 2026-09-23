using StudentPortal.Common.DTOs.User;

namespace StudentPortal.Service.Interfaces;

public interface IUserService
{
    Task<UserResponse> CreateUserAsync(
        CreateUserRequest request,
        CancellationToken ct = default);

    Task<UserResponse> UpdateUserAsync(
        Guid userId,
        UpdateUserRequest request,
        CancellationToken ct = default);
}
