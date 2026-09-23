using StudentPortal.Common.DTOs.AuditLog;
using StudentPortal.Common.DTOs.Shared;
using StudentPortal.Common.Exceptions;
using StudentPortal.Repository.Entities;
using StudentPortal.Repository.Interfaces;
using StudentPortal.Service.Interfaces;

namespace StudentPortal.Service.Implementations;

public class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AuditLogService(
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork)
    {
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<AuditLogListItem>> SearchAsync(
        AuditLogFilter filter,
        CancellationToken ct = default)
    {
        var (items, total) = await _auditLogRepository.SearchAsync(filter, ct);

        return new PagedResult<AuditLogListItem>
        {
            Items = items,
            Total = total,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<AuditLogDetail> GetByIdAsync(
        Guid id,
        CancellationToken ct = default)
    {
        var log = await _auditLogRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Audit log {id} was not found.");

        return new AuditLogDetail
        {
            Id = log.Id,
            UserId = log.UserId,
            UserName = log.User?.UserName,
            Action = log.Action,
            EntityName = log.EntityName,
            EntityId = log.EntityId,
            IpAddress = log.IpAddress,
            CreatedAt = log.CreatedAt,
            OldValue = log.OldValue,
            NewValue = log.NewValue
        };
    }

    public async Task LogAsync(
        string action,
        string entityName,
        string? entityId,
        Guid? userId,
        string? ipAddress,
        CancellationToken ct = default)
    {
        // This SaveChanges does go through the interceptor, but AuditLog is not in the
        // opt-in Include list, so it is not audited again - no loop.
        await _auditLogRepository.AddAsync(new AuditLog
        {
            UserId = userId,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            IpAddress = ipAddress,
            CreatedAt = DateTime.UtcNow
        }, ct);

        await _unitOfWork.SaveChangesAsync(ct);
    }
}
