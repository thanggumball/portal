using StudentPortal.Common.Enums;

namespace StudentPortal.Common.DTOs.Announcement;

// Used for the list screen - does NOT include Content (heavy columns are not pulled into a list).
public class AnnouncementResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Summary { get; init; }
    public AnnouncementStatus Status { get; init; }
    public DateTime? PublishedAt { get; init; }
    public DateTime CreatedAt { get; init; }
}
