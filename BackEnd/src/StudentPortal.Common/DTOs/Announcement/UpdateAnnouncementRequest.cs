namespace StudentPortal.Common.DTOs.Announcement;

public class UpdateAnnouncementRequest
{
    public string Title { get; init; } = string.Empty;
    public string? Summary { get; init; }
    public string Content { get; init; } = string.Empty;
}
