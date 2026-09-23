using Microsoft.EntityFrameworkCore;
using StudentPortal.Repository.Data;
using StudentPortal.Repository.Entities;
using StudentPortal.Repository.Interfaces;

namespace StudentPortal.Repository.Implementations;

public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
{
    public CategoryRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Category>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken ct = default)
    {
        if (ids.Count == 0)
        {
            return Array.Empty<Category>();
        }

        return await _context.Category
            .Where(category => ids.Contains(category.Id))
            .ToListAsync(ct);
    }
}
