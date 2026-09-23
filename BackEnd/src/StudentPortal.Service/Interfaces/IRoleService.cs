using StudentPortal.Common.DTOs.Role;

namespace StudentPortal.Service.Interfaces;

public interface IRoleService
{
    Task<IReadOnlyList<RoleResponse>> GetAllAsync(CancellationToken ct = default);
}
