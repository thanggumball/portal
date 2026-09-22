using MailService.API.Data;
using MailService.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace MailService.API.Repositories;

public class MailAccountRepository : IMailAccountRepository
{
    private readonly MailDbContext _context;

    public MailAccountRepository(MailDbContext context)
    {
        _context = context;
    }

    public async Task<MailAccount?> GetByIdAsync(
        Guid id,
        CancellationToken ct = default)
    {
        return await _context.MailAccounts
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<MailAccount?> GetByEmailAsync(
        string email,
        CancellationToken ct = default)
    {
        return await _context.MailAccounts
            .FirstOrDefaultAsync(x => x.Email == email, ct);
    }

    public async Task AddAsync(
        MailAccount account,
        CancellationToken ct = default)
    {
        await _context.MailAccounts.AddAsync(account, ct);
    }

    public async Task SaveChangesAsync(
        CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
}
