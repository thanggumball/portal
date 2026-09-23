using StudentPortal.Common.DTOs.Shared;

namespace StudentPortal.Common.DTOs.AuditLog;

public class AuditLogFilter : PagingRequest
{
    public Guid? UserId { get; set; }
    public string? EntityName { get; set; }
    public string? Action { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}
