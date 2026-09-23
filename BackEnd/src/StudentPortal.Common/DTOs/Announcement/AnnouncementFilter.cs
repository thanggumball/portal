using StudentPortal.Common.DTOs.Shared;
using StudentPortal.Common.Enums;

namespace StudentPortal.Common.DTOs.Announcement;

public class AnnouncementFilter : PagingRequest
{
    public string? Keyword { get; set; }
    public AnnouncementStatus? Status { get; set; }
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public DateTime? StartDate { get; set; } = DateTime.Now;
    public DateTime? EndDate { get; set; } = DateTime.Now;
}
