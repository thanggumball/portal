using StudentPortal.Common.DTOs.Shared;
using StudentPortal.Common.Enums;

namespace StudentPortal.Common.DTOs.Announcement;

public class AnnouncementFilter : PagingRequest
{
    public AnnouncementStatus? Status { get; set; }
    public string? Keyword { get; set; }
    public AnnouncementRoleReceived? RoleReceived { get; set; }
    public Guid? CategoryId { get; set; }
}
