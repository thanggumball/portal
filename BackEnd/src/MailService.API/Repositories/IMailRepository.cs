using MailService.API.Entities;

namespace MailService.API.Repositories;

public interface IMailRepository
{
    Task<Mail?> GetByIdAsync(
        Guid id,
        CancellationToken ct = default);

    Task<Mail?> GetByMessageIdAsync(
        string messageId,
        CancellationToken ct = default);

    Task<List<Mail>> GetAllAsync(
        CancellationToken ct = default);

    Task AddAsync(
        Mail mail,
        CancellationToken ct = default);

    Task SaveChangesAsync(
        CancellationToken ct = default);
}
