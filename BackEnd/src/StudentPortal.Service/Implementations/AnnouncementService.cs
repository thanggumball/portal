using StudentPortal.Common.DTOs.Announcement;
using StudentPortal.Common.DTOs.Shared;
using StudentPortal.Repository.Entities;
using StudentPortal.Repository.Interfaces;
using StudentPortal.Service.Interfaces;

namespace StudentPortal.Service.Implementations;

public class AnnouncementService : IAnnouncementService
{
    private readonly IAnnouncementRepository _announcementRepository;

    public AnnouncementService(
        IAnnouncementRepository announcementRepository)
    {
        _announcementRepository = announcementRepository;
    }

    public async Task<PagedResult<AnnouncementResponse>> SearchAsync(
        AnnouncementFilter filter,
        CancellationToken ct = default)
    {
        var (announcements, total) =
            await _announcementRepository.SearchAsync(
                filter,
                ct);

        var items = announcements
            .Select(MapToResponse)
            .ToList();

        return new PagedResult<AnnouncementResponse>
        {
            Items = items,
            Total = total,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    private static AnnouncementResponse MapToResponse(
        Announcement announcement)
    {
        return new AnnouncementResponse
        {
            Id = announcement.Id,
            Title = announcement.Title,
            Summary = announcement.Summary,
            Status = announcement.Status,
            RoleReceived = announcement.RoleReceived,
            PublishedAt = announcement.PublishedAt,
            CreatedAt = announcement.CreatedAt,
            UpdatedAt = announcement.UpdatedAt,

            Categories = announcement.AnnouncementCategory
                .OrderBy(link => link.Category.Name)
                .Select(link => new CategoryResponse
                {
                    Id = link.CategoryId,
                    Name = link.Category.Name
                })
                .ToList()
        };
    }

    public Task<AnnouncementDetailResponse> GetByIdAsync(
        Guid id,
        CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<AnnouncementDetailResponse> CreateAsync(
        Guid currentUserId,
        CreateAnnouncementRequest request,
        CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<AnnouncementDetailResponse> UpdateAsync(
        Guid currentUserId,
        Guid id,
        UpdateAnnouncementRequest request,
        CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task PublishAsync(
        Guid currentUserId,
        Guid id,
        CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task SoftDeleteAsync(
        Guid id,
        CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}
