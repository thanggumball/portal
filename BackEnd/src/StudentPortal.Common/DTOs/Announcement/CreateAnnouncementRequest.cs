using StudentPortal.Common.Enums;

namespace StudentPortal.Common.DTOs.Announcement;

public class CreateAnnouncementRequest
{
    public string Title { get; init; } = string.Empty;
    public string? Summary { get; init; }
    public string Content { get; init; } = string.Empty;
    public AnnouncementRoleReceived RoleReceived { get; init; }
    public IReadOnlyList<Guid> CategoryIds { get; init; } = Array.Empty<Guid>();
}
