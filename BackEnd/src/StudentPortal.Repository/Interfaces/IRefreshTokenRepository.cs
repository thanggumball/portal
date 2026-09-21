using StudentPortal.Repository.Entities;

namespace StudentPortal.Repository.Interfaces;

public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
{
    Task<RefreshToken?> FindByTokenHashAsync(
        string tokenHash,
        CancellationToken ct = default);
}