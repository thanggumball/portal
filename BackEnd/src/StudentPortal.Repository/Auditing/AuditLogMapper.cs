using System.Text.Json;
using Audit.Core;
using Audit.EntityFramework;
using StudentPortal.Common.Constants;
using StudentPortal.Common.Helpers;
using StudentPortal.Repository.Entities;

namespace StudentPortal.Repository.Auditing;

/// <summary>
/// Pours ONE Audit.NET event into the columns of the AuditLogs table.
/// This is the only piece of audit logic written by hand.
/// It must NOT throw - every value is null-checked and length-capped.
/// </summary>
public static class AuditLogMapper
{
    // Columns that change on every write, or that already live in their own column -
    // recording them again is pure noise
    private static readonly HashSet<string> NoiseColumns = new(StringComparer.OrdinalIgnoreCase)
    {
        "Id", "CreatedAt", "UpdatedAt"
    };

    /// <returns>true: write this audit row. false: skip it.</returns>
    public static bool TryMap(AuditEvent auditEvent, EventEntry entry, AuditLog log)
    {
        string? oldValue;
        string? newValue;

        switch (entry.Action)
        {
            case "Insert":
                oldValue = null;
                newValue = Serialize(WithoutNoise(entry.ColumnValues));
                break;

            case "Delete":
                oldValue = Serialize(WithoutNoise(entry.ColumnValues));
                newValue = null;
                break;

            case "Update":
                var changes = (entry.Changes ?? new List<EventEntryChange>())
                    .Where(c => !NoiseColumns.Contains(c.ColumnName))
                    .ToList();

                // Only noise columns or ignored columns changed
                // (e.g. LastLoginAt on login, PasswordHash on a password change) -> skip
                if (changes.Count == 0)
                {
                    return false;
                }

                oldValue = Serialize(changes.ToDictionary(c => c.ColumnName, c => c.OriginalValue));
                newValue = Serialize(changes.ToDictionary(c => c.ColumnName, c => c.NewValue));
                break;

            default:
                return false;
        }

        log.UserId = ReadGuid(auditEvent, AuditFields.UserId);
        log.Action = ResolveAction(entry);
        log.EntityName = Truncate(entry.Name ?? entry.Table ?? "Unknown", 100)!;
        log.EntityId = Truncate(entry.PrimaryKey?.Values.FirstOrDefault()?.ToString(), 100);
        log.OldValue = oldValue;
        log.NewValue = newValue;
        log.IpAddress = Truncate(ReadString(auditEvent, AuditFields.IpAddress), 45);
        log.CreatedAt = DateTime.UtcNow;

        return true;
    }

    private static string ResolveAction(EventEntry entry) => entry.Action switch
    {
        "Insert" => "Create",
        "Delete" => "Delete",
        "Update" when IsSoftDelete(entry) => "SoftDelete",   // a soft delete is technically an UPDATE
        _ => "Update"
    };

    private static bool IsSoftDelete(EventEntry entry) =>
        entry.Changes?.Any(c =>
            string.Equals(c.ColumnName, "IsDeleted", StringComparison.OrdinalIgnoreCase)
            && (c.NewValue is true
                || string.Equals(c.NewValue?.ToString(), "True", StringComparison.OrdinalIgnoreCase)))
        == true;

    private static Dictionary<string, object?> WithoutNoise(IDictionary<string, object?>? values) =>
        values is null
            ? new Dictionary<string, object?>()
            : values.Where(kv => !NoiseColumns.Contains(kv.Key))
                    .ToDictionary(kv => kv.Key, kv => kv.Value);

    private static string? Serialize<T>(IDictionary<string, T> values) =>
        values.Count == 0 ? null : JsonSerializer.Serialize(values, AuditJson.Options);

    private static Guid? ReadGuid(AuditEvent auditEvent, string key) =>
        Guid.TryParse(ReadString(auditEvent, key), out var id) ? id : null;

    private static string? ReadString(AuditEvent auditEvent, string key) =>
        auditEvent.CustomFields.TryGetValue(key, out var value) ? value?.ToString() : null;

    // Cap at the column length in the schema, so one oversized value cannot break the whole row
    private static string? Truncate(string? value, int max) =>
        value is null || value.Length <= max ? value : value[..max];
}
