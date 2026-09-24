using StudentPortal.Common.DTOs.User;
using StudentPortal.Repository.Entities;

namespace StudentPortal.Repository.Interfaces;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetForLoginAsync(string email, CancellationToken ct = default);
    Task<User?> FindByUserNameAsync(string userName, CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
    Task<(IReadOnlyList<User> Items, int Total)> SearchAsync(UserFilter filter, CancellationToken ct = default);
    Task<User?> GetByIdWithRoleAsync(Guid id,CancellationToken ct = default);
    Task<IReadOnlyDictionary<Guid, string>> GetUserNamesAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
}
