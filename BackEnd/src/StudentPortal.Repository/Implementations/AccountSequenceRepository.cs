using Microsoft.EntityFrameworkCore;
using StudentPortal.Repository.Data;
using StudentPortal.Repository.Entities;
using StudentPortal.Repository.Interfaces;

namespace StudentPortal.Repository.Implementations;

public class AccountSequenceRepository
    : GenericRepository<AccountSequence>,
      IAccountSequenceRepository
{
    public AccountSequenceRepository(AppDbContext context)
        : base(context)
    {
    }

    public async Task<AccountSequence?> GetByAccountTypeAsync(
        string accountType,
        CancellationToken ct = default)
        => await _context.AccountSequences
            .FirstOrDefaultAsync(
                x => x.AccountType == accountType,
                ct);

    public async Task<AccountSequence?> GetForUpdateAsync(
        string accountType,
        CancellationToken ct = default)
        => await _context.AccountSequences
            .FromSqlInterpolated($"""
                SELECT *
                FROM [AccountSequences] WITH (UPDLOCK, ROWLOCK)
                WHERE [AccountType] = {accountType}
                """)
            .AsTracking()
            .FirstOrDefaultAsync(ct);
}
