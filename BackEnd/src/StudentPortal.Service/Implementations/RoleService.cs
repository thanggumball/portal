using StudentPortal.Common.DTOs.Role;
using StudentPortal.Repository.Interfaces;
using StudentPortal.Service.Interfaces;

namespace StudentPortal.Service.Implementations;

public class RoleService : IRoleService
{
    private readonly IRoleRepository _roleRepository;

    public RoleService(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<IReadOnlyList<RoleResponse>> GetAllAsync(
        CancellationToken ct = default)
    {
        var roles = await _roleRepository.GetAllAsync(ct);
        return roles
            .Select(r => new RoleResponse { Id = r.Id, Name = r.Name })
            .ToList();
    }
}
