namespace StudentPortal.Common.DTOs.AuditLog;

/// <summary>One changed field, values already converted to display text.</summary>
public class AuditLogChange
{
    public string Field { get; init; } = string.Empty;
    public string? OldValue { get; init; }
    public string? NewValue { get; init; }
}
