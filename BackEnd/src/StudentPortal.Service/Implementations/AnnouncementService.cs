using StudentPortal.Common.DTOs.Announcement;
using StudentPortal.Common.DTOs.Shared;
using StudentPortal.Common.Enums;
using StudentPortal.Common.Exceptions;
using StudentPortal.Repository.Entities;
using StudentPortal.Repository.Interfaces;
using StudentPortal.Service.Interfaces;

namespace StudentPortal.Service.Implementations;

public class AnnouncementService : IAnnouncementService
{
    private readonly IAnnouncementRepository _announcementRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AnnouncementService(
        IAnnouncementRepository announcementRepository,
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _announcementRepository = announcementRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
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

    public Task<AnnouncementDetailResponse> GetByIdAsync(
        Guid id,
        CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public async Task<AnnouncementDetailResponse> CreateAsync(
        Guid currentUserId,
        CreateAnnouncementRequest request,
        CancellationToken ct = default)
    {
        var categoryIds = request.CategoryIds
            .Distinct()
            .ToList();

        var categories = await _categoryRepository.GetByIdsAsync(
            categoryIds,
            ct);

        if (categories.Count != categoryIds.Count)
        {
            var existingCategoryIds = categories
                .Select(category => category.Id)
                .ToHashSet();

            var missingCategoryIds = categoryIds
                .Where(id => !existingCategoryIds.Contains(id));

            throw new BadRequestException(
                $"Categories were not found: " +
                $"{string.Join(", ", missingCategoryIds)}.");
        }

        var now = DateTime.UtcNow;

        var announcement = new Announcement
        {
            Title = request.Title.Trim(),

            Summary = string.IsNullOrWhiteSpace(request.Summary)
                ? null
                : request.Summary.Trim(),

            Content = request.Content.Trim(),

            RoleReceived = request.RoleReceived,

            Status = AnnouncementStatus.Draft,

            PublishedAt = null,

            CreatedBy = currentUserId,

            UpdatedBy = null,

            IsDeleted = false,

            CreatedAt = now,

            UpdatedAt = now
        };

        foreach (var category in categories)
        {
            announcement.AnnouncementCategory.Add(
                new AnnouncementCategory
                {
                    Announcement = announcement,
                    CategoryId = category.Id,
                    Category = category,
                    CreatedAt = now
                });
        }

        await _announcementRepository.AddAsync(
            announcement,
            ct);

        await _unitOfWork.SaveChangesAsync(ct);

        return MapToDetailResponse(announcement);
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
            Categories = MapCategories(announcement)
        };
    }

    private static AnnouncementDetailResponse MapToDetailResponse(
        Announcement announcement)
    {
        return new AnnouncementDetailResponse
        {
            Id = announcement.Id,
            Title = announcement.Title,
            Summary = announcement.Summary,
            Content = announcement.Content,
            Status = announcement.Status,
            RoleReceived = announcement.RoleReceived,
            PublishedAt = announcement.PublishedAt,
            CreatedAt = announcement.CreatedAt,
            UpdatedAt = announcement.UpdatedAt,
            Categories = MapCategories(announcement)
        };
    }

    private static List<CategoryResponse> MapCategories(
        Announcement announcement)
    {
        return announcement.AnnouncementCategory
            .OrderBy(link => link.Category.Name)
            .Select(link => new CategoryResponse
            {
                Id = link.CategoryId,
                Name = link.Category.Name
            })
            .ToList();
    }
}
