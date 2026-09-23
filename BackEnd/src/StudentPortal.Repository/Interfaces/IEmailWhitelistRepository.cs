using StudentPortal.Repository.Entities;

namespace StudentPortal.Repository.Interfaces;

public interface IEmailWhitelistRepository : IGenericRepository<EmailWhitelist>
{
    Task<EmailWhitelist?> FindByEmailAsync(
        string email,
        CancellationToken ct = default);

    Task<EmailWhitelist?> GetAvailableByEmailAsync(
        string email,
        CancellationToken ct = default);
}