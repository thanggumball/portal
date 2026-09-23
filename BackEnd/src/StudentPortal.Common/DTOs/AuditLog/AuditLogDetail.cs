namespace StudentPortal.Common.DTOs.AuditLog;

/// <summary>Detail row - the full record, only fetched one at a time.</summary>
public class AuditLogDetail : AuditLogListItem
{
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
}
