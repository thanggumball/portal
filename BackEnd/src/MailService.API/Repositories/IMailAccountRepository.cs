using MailService.API.Entities;

namespace MailService.API.Repositories;

public interface IMailAccountRepository
{
    Task<MailAccount?> GetByIdAsync(
        Guid id,
        CancellationToken ct = default);

    Task<MailAccount?> GetByEmailAsync(
        string email,
        CancellationToken ct = default);

    Task AddAsync(
        MailAccount account,
        CancellationToken ct = default);

    Task SaveChangesAsync(
        CancellationToken ct = default);
}
