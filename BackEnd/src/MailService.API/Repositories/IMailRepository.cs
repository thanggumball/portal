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

    Task<List<Mail>> GetInboxAsync(
        string email,
        CancellationToken ct = default);

    Task<List<Mail>> GetSentAsync(
        string email,
        CancellationToken ct = default);

    Task AddAsync(
        Mail mail,
        CancellationToken ct = default);

    Task SaveChangesAsync(
        CancellationToken ct = default);
}
