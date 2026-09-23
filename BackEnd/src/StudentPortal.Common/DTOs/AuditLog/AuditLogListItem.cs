namespace StudentPortal.Common.DTOs.AuditLog;

/// <summary>List row - deliberately without OldValue / NewValue (both nvarchar(max)).</summary>
public class AuditLogListItem
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; }
}
