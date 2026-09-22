using StudentPortal.Repository.Entities;

namespace StudentPortal.Repository.Interfaces;

public interface IAccountSequenceRepository
    : IGenericRepository<AccountSequence>
{
    Task<AccountSequence?> GetByAccountTypeAsync(
        string accountType,
        CancellationToken ct = default);

    Task<AccountSequence?> GetForUpdateAsync(
        string accountType,
        CancellationToken ct = default);
}
