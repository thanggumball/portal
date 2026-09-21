using StudentPortal.Common.DTOs.Shared;
using StudentPortal.Common.DTOs.User;

namespace StudentPortal.Service.Interfaces;

public interface IUserService
{
    Task<PagedResult<UserResponse>> SearchAsync(UserFilter filter, CancellationToken ct = default);
    Task<UserResponse> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken ct = default);
    Task<UserResponse> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken ct = default);
    Task UpdateStatusAsync(Guid id, UpdateUserStatusRequest request, CancellationToken ct = default);
    Task SoftDeleteAsync(Guid id, CancellationToken ct = default);
}
