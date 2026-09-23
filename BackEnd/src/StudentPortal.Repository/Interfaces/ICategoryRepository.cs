using StudentPortal.Repository.Entities;

namespace StudentPortal.Repository.Interfaces;

public interface ICategoryRepository : IGenericRepository<Category>
{
    Task<IReadOnlyList<Category>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken ct = default);
}
