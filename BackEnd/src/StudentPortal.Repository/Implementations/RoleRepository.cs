using Microsoft.EntityFrameworkCore;
using StudentPortal.Repository.Data;
using StudentPortal.Repository.Entities;
using StudentPortal.Repository.Interfaces;

namespace StudentPortal.Repository.Implementations;

public class RoleRepository : GenericRepository<Role>, IRoleRepository
{
    public RoleRepository(AppDbContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<Role>> GetAllAsync(
        CancellationToken ct = default)
        => await _context.Roles
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<Role?> FindByNameAsync(
        string name,
        CancellationToken ct = default)
        => await _context.Roles
            .FirstOrDefaultAsync(r => r.Name == name, ct);
}