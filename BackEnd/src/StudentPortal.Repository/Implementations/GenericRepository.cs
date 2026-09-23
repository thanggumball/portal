using Microsoft.EntityFrameworkCore;
using StudentPortal.Repository.Data;
using StudentPortal.Repository.Interfaces;

namespace StudentPortal.Repository.Implementations;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly AppDbContext _context;

    public GenericRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Set<T>().FindAsync(new object[] { id }, ct);

    public async Task<IReadOnlyList<T>> GetAllAsync(
        CancellationToken ct = default)
        => await _context.Set<T>()
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task AddAsync(T entity, CancellationToken ct = default)
        => await _context.Set<T>().AddAsync(entity, ct);

    public void Update(T entity)
        => _context.Set<T>().Update(entity);

    public void Remove(T entity)
        => _context.Set<T>().Remove(entity);
}
