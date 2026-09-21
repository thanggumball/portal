using StudentPortal.Repository.Entities;

namespace StudentPortal.Repository.Interfaces;

public interface IRoleRepository : IGenericRepository<Role>
{
    // Roles is a small, fixed lookup table - the one exception allowed to have GetAllAsync().
    Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken ct = default);
    Task<Role?> FindByNameAsync(string name, CancellationToken ct = default);
}
