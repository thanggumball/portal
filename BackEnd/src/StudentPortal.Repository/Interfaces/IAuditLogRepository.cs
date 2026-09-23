using StudentPortal.Common.DTOs.AuditLog;
using StudentPortal.Repository.Entities;

namespace StudentPortal.Repository.Interfaces;

/// <summary>
/// Deliberately has NO Update and NO Remove - once an audit row is written it is never
/// changed or deleted. That is why this interface does NOT extend IGenericRepository.
/// </summary>
public interface IAuditLogRepository
{
    Task AddAsync(AuditLog log, CancellationToken ct = default);
    Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(IReadOnlyList<AuditLogListItem> Items, int Total)> SearchAsync(
        AuditLogFilter filter, CancellationToken ct = default);
}
