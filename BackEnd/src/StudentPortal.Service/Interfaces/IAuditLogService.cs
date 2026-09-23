using StudentPortal.Common.DTOs.AuditLog;
using StudentPortal.Common.DTOs.Shared;

namespace StudentPortal.Service.Interfaces;

public interface IAuditLogService
{
    Task<PagedResult<AuditLogListItem>> SearchAsync(
        AuditLogFilter filter,
        CancellationToken ct = default);

    Task<AuditLogDetail> GetByIdAsync(
        Guid id,
        CancellationToken ct = default);

    /// <summary>
    /// Writes an audit row by hand, for events Audit.NET cannot see because no data
    /// changed (a failed login) or because the changed column is ignored (a password change).
    /// </summary>
    Task LogAsync(
        string action,
        string entityName,
        string? entityId,
        Guid? userId,
        string? ipAddress,
        CancellationToken ct = default);
}
