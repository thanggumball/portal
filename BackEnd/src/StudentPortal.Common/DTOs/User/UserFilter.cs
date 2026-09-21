using StudentPortal.Common.DTOs.Shared;

namespace StudentPortal.Common.DTOs.User;

public class UserFilter : PagingRequest
{
    public Guid? RoleId { get; set; }
    public string? Keyword { get; set; }
}
