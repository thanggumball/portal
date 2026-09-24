using StudentPortal.Common.DTOs.AuditLog;
using StudentPortal.Common.DTOs.Shared;
using StudentPortal.Common.Exceptions;
using StudentPortal.Repository.Entities;
using StudentPortal.Repository.Interfaces;
using StudentPortal.Service.Helpers;
using StudentPortal.Service.Interfaces;

namespace StudentPortal.Service.Implementations;

public class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AuditLogService(
        IAuditLogRepository auditLogRepository,
        IRoleRepository roleRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _auditLogRepository = auditLogRepository;
        _roleRepository = roleRepository;
        _userRepository = userRepository;
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

        // Look up names here so the client gets "Staff" / "admin" instead of raw Guids
        var roleNames = (await _roleRepository.GetAllAsync(ct)).ToDictionary(r => r.Id, r => r.Name);

        var userIds = AuditChangeBuilder.CollectGuids(log.OldValue, log.NewValue, AuditChangeBuilder.UserReferenceFields);
        var userNames = await _userRepository.GetUserNamesAsync(userIds, ct);

        return new AuditLogDetail
        {
            Id = log.Id,
            UserId = log.UserId,
            UserName = log.User?.UserName,
            Action = log.Action,
            EntityName = log.EntityName,
            EntityId = log.EntityId,
            IpAddress = log.IpAddress,
            // Stored as UTC, but EF reads it back as Unspecified - without this the JSON has no "Z"
            CreatedAt = DateTime.SpecifyKind(log.CreatedAt, DateTimeKind.Utc),
            Changes = AuditChangeBuilder.Build(log.EntityName, log.OldValue, log.NewValue, roleNames, userNames)
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
