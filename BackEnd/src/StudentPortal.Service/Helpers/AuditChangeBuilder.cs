using System.Text.Json;
using StudentPortal.Common.DTOs.AuditLog;
using StudentPortal.Common.Enums;

namespace StudentPortal.Service.Helpers;

/// <summary>
/// Turns the raw OldValue / NewValue JSON of an audit log into a list of changed fields
/// with display-ready values. Pure - no database access - so it is easy to unit test.
/// </summary>
public static class AuditChangeBuilder
{
    /// <summary>Guid-valued columns that point to a user, per the audited entities.</summary>
    public static readonly string[] UserReferenceFields = { "CreatedBy", "UpdatedBy" };

    /// <summary>Collects every Guid in old/new JSON under the given field names (for the name lookup).</summary>
    public static IReadOnlyCollection<Guid> CollectGuids(
        string? oldJson,
        string? newJson,
        IEnumerable<string> fieldNames)
    {
        var names = fieldNames.ToHashSet();
        var result = new HashSet<Guid>();

        foreach (var values in new[] { Parse(oldJson), Parse(newJson) })
        {
            foreach (var (field, element) in values)
            {
                if (names.Contains(field) && TryGetGuid(element, out var id))
                    result.Add(id);
            }
        }

        return result;
    }

    public static IReadOnlyList<AuditLogChange> Build(
        string entityName,
        string? oldJson,
        string? newJson,
        IReadOnlyDictionary<Guid, string> roleNames,
        IReadOnlyDictionary<Guid, string> userNames)
    {
        var oldValues = Parse(oldJson);
        var newValues = Parse(newJson);

        // New first, then fields that only exist in old - keeps the order they were written in
        var fields = newValues.Keys.Concat(oldValues.Keys).Distinct();
        var changes = new List<AuditLogChange>();

        foreach (var field in fields)
        {
            JsonElement? oldElement = oldValues.TryGetValue(field, out var o) ? o : null;
            JsonElement? newElement = newValues.TryGetValue(field, out var n) ? n : null;

            // Older logs (written before the mapper fix) also list columns that did not change
            if (oldElement?.GetRawText() == newElement?.GetRawText())
                continue;

            changes.Add(new AuditLogChange
            {
                Field = field,
                OldValue = FormatValue(entityName, field, oldElement, roleNames, userNames),
                NewValue = FormatValue(entityName, field, newElement, roleNames, userNames)
            });
        }

        return changes;
    }

    // A broken log must never break the API, so bad JSON is treated as "no values"
    private static Dictionary<string, JsonElement> Parse(string? json)
    {
        var result = new Dictionary<string, JsonElement>();
        if (string.IsNullOrWhiteSpace(json)) return result;

        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind != JsonValueKind.Object) return result;

            // Clone so the elements outlive the disposed document
            foreach (var property in doc.RootElement.EnumerateObject())
                result[property.Name] = property.Value.Clone();
        }
        catch (JsonException)
        {
            // ignore - treated as empty
        }

        return result;
    }

    private static string? FormatValue(
        string entityName,
        string field,
        JsonElement? element,
        IReadOnlyDictionary<Guid, string> roleNames,
        IReadOnlyDictionary<Guid, string> userNames)
    {
        if (element is not { } e || e.ValueKind == JsonValueKind.Null)
            return null;

        if (field == "RoleId" && TryGetGuid(e, out var roleId))
            return roleNames.TryGetValue(roleId, out var roleName) ? roleName : e.GetString();

        if (UserReferenceFields.Contains(field) && TryGetGuid(e, out var userId))
            return userNames.TryGetValue(userId, out var userName) ? userName : e.GetString();

        // Enums are stored as numbers in the log JSON
        var enumType = (entityName, field) switch
        {
            ("User", "Status") => typeof(UserStatus),
            ("Announcement", "Status") => typeof(AnnouncementStatus),
            (_, "RoleReceived") => typeof(AnnouncementRoleReceived),
            _ => null
        };

        if (enumType != null && e.ValueKind == JsonValueKind.Number && e.TryGetInt32(out var number))
            return Enum.GetName(enumType, number) ?? number.ToString();

        return e.ValueKind switch
        {
            JsonValueKind.True => "Yes",
            JsonValueKind.False => "No",
            JsonValueKind.String => e.GetString(),
            _ => e.GetRawText()
        };
    }

    private static bool TryGetGuid(JsonElement element, out Guid id)
    {
        id = Guid.Empty;
        return element.ValueKind == JsonValueKind.String && Guid.TryParse(element.GetString(), out id);
    }
}
