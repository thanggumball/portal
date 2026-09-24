using Microsoft.EntityFrameworkCore;
using StudentPortal.Common.DTOs.User;
using StudentPortal.Repository.Data;
using StudentPortal.Repository.Entities;
using StudentPortal.Repository.Interfaces;

namespace StudentPortal.Repository.Implementations;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context)
        : base(context)
    {
    }

    public async Task<User?> GetForLoginAsync(
        string email,
        CancellationToken ct = default)
        => await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(
                u => u.Email == email && !u.IsDeleted,
                ct);

    public async Task<User?> FindByUserNameAsync(
        string userName,
        CancellationToken ct = default)
        => await _context.Users
            .FirstOrDefaultAsync(
                u => u.UserName == userName && !u.IsDeleted,
                ct);

    public async Task<bool> ExistsByEmailAsync(
        string email,
        CancellationToken ct = default)
        => await _context.Users
            .AnyAsync(
                u => u.Email == email && !u.IsDeleted,
                ct);

    public async Task<(IReadOnlyList<User> Items, int Total)> SearchAsync(
    UserFilter filter,
    CancellationToken ct = default)
    {
        var query = _context.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .Where(u => !u.IsDeleted);

        if (filter.RoleId.HasValue)
        {
            query = query.Where(u => u.RoleId == filter.RoleId.Value);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(u => u.Status == filter.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Email))
        { 
            query = query.Where(u => u.Email.Contains(filter.Email));
        }

        if (!string.IsNullOrWhiteSpace(filter.UserName))
        {
            query = query.Where(u => u.UserName.Contains(filter.UserName));
        }

        if (!string.IsNullOrWhiteSpace(filter.FullName))
        {
            query = query.Where(u => u.FullName.Contains(filter.FullName));
        }

        if (!string.IsNullOrWhiteSpace(filter.UserCode))
        {
            query = query.Where(u => u.UserCode != null && u.UserCode.Contains(filter.UserCode));
        }

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            var kw = filter.Keyword;
            query = query.Where(u =>
                u.Email.Contains(kw) ||
                u.UserName.Contains(kw) ||
                u.FullName.Contains(kw) ||
                (u.UserCode != null && u.UserCode.Contains(kw)));
        }

        if (filter.CreatedFrom.HasValue)
        {
            query = query.Where(u => u.CreatedAt >= filter.CreatedFrom.Value);
        }

        if (filter.CreatedTo.HasValue)
        {
            query = query.Where(u => u.CreatedAt <= filter.CreatedTo.Value);
        }

        if (filter.LastLoginFrom.HasValue)
        {
            query = query.Where(u => u.LastLoginAt >= filter.LastLoginFrom.Value);
        }

        if (filter.LastLoginTo.HasValue)
        {
            query = query.Where(u => u.LastLoginAt <= filter.LastLoginTo.Value);
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<User?> GetByIdWithRoleAsync(
    Guid id,
    CancellationToken ct = default)
    => await _context.Users
        .Include(u => u.Role)
        .FirstOrDefaultAsync(
            u => u.Id == id && !u.IsDeleted,
            ct);

    // Deliberately includes soft-deleted users - old audit logs can still point to them
    public async Task<IReadOnlyDictionary<Guid, string>> GetUserNamesAsync(
        IEnumerable<Guid> ids,
        CancellationToken ct = default)
    {
        var idList = ids.Distinct().ToList();
        if (idList.Count == 0) return new Dictionary<Guid, string>();

        return await _context.Users
            .AsNoTracking()
            .Where(u => idList.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.UserName, ct);
    }
}
