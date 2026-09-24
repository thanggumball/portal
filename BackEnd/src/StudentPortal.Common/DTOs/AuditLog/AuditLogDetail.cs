namespace StudentPortal.Common.DTOs.AuditLog;

/// <summary>Detail row - the full record, only fetched one at a time.</summary>
public class AuditLogDetail : AuditLogListItem
{
    /// <summary>Only the fields whose value changed, ready to display.</summary>
    public IReadOnlyList<AuditLogChange> Changes { get; set; } = Array.Empty<AuditLogChange>();
}
