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
        throw new NotImplementedException();
    }

    public async Task<User?> GetByIdWithRoleAsync(
    Guid id,
    CancellationToken ct = default)
    => await _context.Users
        .Include(u => u.Role)
        .FirstOrDefaultAsync(
            u => u.Id == id && !u.IsDeleted,
            ct);
}