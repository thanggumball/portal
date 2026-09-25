using System.Text.Json;
using StudentPortal.Common.DTOs.AuditLog;
using StudentPortal.Common.DTOs.Shared;
using StudentPortal.Common.Enums;
using StudentPortal.Common.Exceptions;
using StudentPortal.Repository.Entities;
using StudentPortal.Repository.Interfaces;
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

        // Step 1: JSON -> plain text per field, e.g. { "Status": "3", "FullName": "An" }
        var oldValues = ReadJson(log.OldValue);
        var newValues = ReadJson(log.NewValue);

        // Step 2: load role and user names, so Guids can be shown as names
        var roleNames = (await _roleRepository.GetAllAsync(ct))
            .ToDictionary(r => r.Id, r => r.Name);

        var userNames = await _userRepository.GetUserNamesAsync(
            GetUserIds(oldValues, newValues),
            ct);

        // Step 3: compare field by field, keep only the fields that changed
        var changes = new List<AuditLogChange>();

        foreach (var field in newValues.Keys.Union(oldValues.Keys))
        {
            oldValues.TryGetValue(field, out var oldValue);
            newValues.TryGetValue(field, out var newValue);

            // Older logs (written before the mapper fix) also contain columns that did not change
            if (oldValue == newValue)
            {
                continue;
            }

            changes.Add(new AuditLogChange
            {
                Field = field,
                OldValue = ToReadable(log.EntityName, field, oldValue, roleNames, userNames),
                NewValue = ToReadable(log.EntityName, field, newValue, roleNames, userNames)
            });
        }

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
            Changes = changes
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

    // {"Status":3,"IsDeleted":false} -> { "Status": "3", "IsDeleted": "false" }
    // Empty or broken JSON -> empty dictionary, so one bad log cannot break the API
    private static Dictionary<string, string?> ReadJson(string? json)
    {
        var result = new Dictionary<string, string?>();

        if (string.IsNullOrWhiteSpace(json))
        {
            return result;
        }

        try
        {
            var values = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

            if (values is null)
            {
                return result;
            }

            foreach (var (field, value) in values)
            {
                if (value.ValueKind == JsonValueKind.Null)
                {
                    result[field] = null;
                }
                else if (value.ValueKind == JsonValueKind.String)
                {
                    result[field] = value.GetString();
                }
                else
                {
                    // Numbers and true / false
                    result[field] = value.GetRawText();
                }
            }
        }
        catch (JsonException)
        {
            // Broken log -> treat it as "no values"
        }

        return result;
    }

    // Turns one stored value into text a person can read
    private static string? ToReadable(
        string entityName,
        string field,
        string? value,
        Dictionary<Guid, string> roleNames,
        IReadOnlyDictionary<Guid, string> userNames)
    {
        if (value is null)
        {
            return null;
        }

        if (field == "RoleId" && Guid.TryParse(value, out var roleId) && roleNames.ContainsKey(roleId))
        {
            return roleNames[roleId];
        }

        if ((field == "CreatedBy" || field == "UpdatedBy")
            && Guid.TryParse(value, out var userId)
            && userNames.ContainsKey(userId))
        {
            return userNames[userId];
        }

        // Enums are stored as numbers in the log. A number that is not in the enum is shown as it is.
        if (field == "Status" && entityName == "User" && int.TryParse(value, out var userStatus))
        {
            return Enum.GetName(typeof(UserStatus), userStatus) ?? value;
        }

        if (field == "Status" && entityName == "Announcement" && int.TryParse(value, out var announcementStatus))
        {
            return Enum.GetName(typeof(AnnouncementStatus), announcementStatus) ?? value;
        }

        if (field == "RoleReceived" && int.TryParse(value, out var roleReceived))
        {
            return Enum.GetName(typeof(AnnouncementRoleReceived), roleReceived) ?? value;
        }

        if (value == "true")
        {
            return "Yes";
        }

        if (value == "false")
        {
            return "No";
        }

        return value;
    }

    // Collects the Guids in CreatedBy / UpdatedBy, so all user names are loaded in one query
    private static List<Guid> GetUserIds(
        Dictionary<string, string?> oldValues,
        Dictionary<string, string?> newValues)
    {
        var ids = new List<Guid>();

        foreach (var values in new[] { oldValues, newValues })
        {
            foreach (var field in new[] { "CreatedBy", "UpdatedBy" })
            {
                if (values.TryGetValue(field, out var value) && Guid.TryParse(value, out var id))
                {
                    ids.Add(id);
                }
            }
        }

        return ids;
    }
}
