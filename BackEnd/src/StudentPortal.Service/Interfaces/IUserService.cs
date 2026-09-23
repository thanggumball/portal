using StudentPortal.Common.DTOs.Shared;
using StudentPortal.Common.DTOs.User;

namespace StudentPortal.Service.Interfaces;

public interface IUserService
{
    Task<PagedResult<UserResponse>> SearchUsersAsync(
        UserFilter filter,
        CancellationToken ct = default);

    Task<UserResponse> CreateUserAsync(
        CreateUserRequest request,
        CancellationToken ct = default);

    Task<UserResponse> UpdateUserAsync(
        Guid userId,
        UpdateUserRequest request,
        CancellationToken ct = default);
}
