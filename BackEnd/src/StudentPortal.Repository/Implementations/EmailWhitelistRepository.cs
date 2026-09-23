using Microsoft.EntityFrameworkCore;
using StudentPortal.Repository.Data;
using StudentPortal.Repository.Entities;
using StudentPortal.Repository.Interfaces;

namespace StudentPortal.Repository.Implementations;

public class EmailWhitelistRepository
    : GenericRepository<EmailWhitelist>, IEmailWhitelistRepository
{
    public EmailWhitelistRepository(AppDbContext context)
        : base(context)
    {
    }

    public async Task<EmailWhitelist?> FindByEmailAsync(
        string email,
        CancellationToken ct = default)
        => await _context.EmailWhitelists
            .FirstOrDefaultAsync(
                e => e.Email == email,
                ct);

    public async Task<EmailWhitelist?> GetAvailableByEmailAsync(
        string email,
        CancellationToken ct = default)
        => await _context.EmailWhitelists
            .Include(e => e.Role)
            .FirstOrDefaultAsync(
                e => e.Email == email && !e.IsUsed,
                ct);
}