using StudentPortal.Common.DTOs.Announcement;
using StudentPortal.Common.DTOs.Shared;

namespace StudentPortal.Service.Interfaces;

public interface IAnnouncementService
{
    Task<PagedResult<AnnouncementResponse>> SearchAsync(
        AnnouncementFilter filter,
        Guid? userId,
        CancellationToken ct = default);

    Task<AnnouncementDetailResponse> GetByIdAsync(
        Guid id,
        Guid userId,
        CancellationToken ct = default);

    Task<AnnouncementDetailResponse> CreateAsync(
        Guid currentUserId,
        CreateAnnouncementRequest request,
        CancellationToken ct = default);

    Task<AnnouncementDetailResponse> UpdateAsync(
        Guid currentUserId,
        Guid id,
        UpdateAnnouncementRequest request,
        CancellationToken ct = default);

    Task PublishAsync(
        Guid currentUserId,
        Guid id,
        CancellationToken ct = default);

    Task SoftDeleteAsync(
        Guid id,
        CancellationToken ct = default);
}
