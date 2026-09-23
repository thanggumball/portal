using StudentPortal.Common.DTOs.Announcement;
using StudentPortal.Common.DTOs.Shared;
using StudentPortal.Common.Enums;

namespace StudentPortal.Service.Interfaces;

public interface IAnnouncementService
{
    Task<PagedResult<AnnouncementResponse>> SearchAsync(AnnouncementFilter filter, Guid userId, CancellationToken ct = default);
    Task<AnnouncementDetailResponse> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default);
}
