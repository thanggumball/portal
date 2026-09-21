namespace StudentPortal.Common.DTOs.Announcement;

// Used for the detail screen - adds Content.
public class AnnouncementDetailResponse : AnnouncementResponse
{
    public string Content { get; init; } = string.Empty;
}
