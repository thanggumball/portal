using StudentPortal.Common.DTOs.Shared;
using StudentPortal.Common.Enums;

namespace StudentPortal.Common.DTOs.User;

public class UserFilter : PagingRequest
{
    public Guid? RoleId { get; set; }
    public UserStatus? Status { get; set; }
    public string? Email { get; set; }
    public string? UserName { get; set; }
    public string? FullName { get; set; }
    public string? UserCode { get; set; }
    public string? Keyword { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public DateTime? LastLoginFrom { get; set; }
    public DateTime? LastLoginTo { get; set; }
}
