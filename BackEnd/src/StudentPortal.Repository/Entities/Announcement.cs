using StudentPortal.Common.Enums;

namespace StudentPortal.Repository.Entities;

public class Announcement : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string Content { get; set; } = string.Empty;
    public AnnouncementStatus Status { get; set; }
    public DateTime? PublishedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User Creator { get; set; } = null!;
    public User? Updater { get; set; }
}
