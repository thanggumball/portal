using StudentPortal.Repository.Entities;

namespace StudentPortal.Repository.Interfaces;

public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
{
    Task<RefreshToken?> FindActiveByTokenHashAsync(string tokenHash, CancellationToken ct = default);
}
