using StudentPortal.Common.DTOs.Announcement;
using StudentPortal.Common.DTOs.Category;
using StudentPortal.Common.DTOs.Shared;
using StudentPortal.Common.Enums;
using StudentPortal.Common.Exceptions;
using StudentPortal.Repository.Entities;
using StudentPortal.Repository.Interfaces;
using StudentPortal.Service.Interfaces;

namespace StudentPortal.Service.Implementations;

public class AnnouncementService : IAnnouncementService
{
    private readonly IAnnouncementRepository
        _announcementRepository;

    private readonly IUserRepository _userRepository;

    private readonly ICategoryRepository _categoryRepository;

    private readonly IUnitOfWork _unitOfWork;

    public AnnouncementService(
        IAnnouncementRepository announcementRepository,
        IUserRepository userRepository,
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _announcementRepository = announcementRepository;
        _userRepository = userRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    // Andrew: Get List
    public async Task<PagedResult<AnnouncementResponse>> SearchAsync(
        AnnouncementFilter filterRequest,
        Guid userId,
        CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdWithRoleAsync(
            userId,
            ct);

        if (user is null)
        {
            throw new NotFoundException("User not found.");
        }

        if (user.Role is null)
        {
            throw new ForbiddenException(
                "The user has not been assigned a role.");
        }

        var userRole = MapUserRole(user.Role.Name);

        var (items, total) =
            await _announcementRepository.SearchAsync(
                filterRequest,
                userRole,
                ct);

        return new PagedResult<AnnouncementResponse>
        {
            Items = items
                .Select(MapToResponse)
                .ToList(),

            Total = total,
            Page = filterRequest.Page,
            PageSize = filterRequest.PageSize
        };
    }

    // Andrew: Get Detail
    public async Task<AnnouncementDetailResponse> GetByIdAsync(
        Guid announcementId,
        Guid userId,
        CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdWithRoleAsync(
            userId,
            ct);

        if (user is null)
        {
            throw new NotFoundException("User not found.");
        }

        if (user.Role is null)
        {
            throw new ForbiddenException(
                "The user has not been assigned a role.");
        }

        var userRole = MapUserRole(user.Role.Name);

        var announcement =
            await _announcementRepository
                .GetDetailedByIdAsync(
                    announcementId,
                    userRole,
                    ct);

        if (announcement is null)
        {
            throw new NotFoundException(
                "Announcement not found.");
        }

        return MapToDetailResponse(announcement);
    }

    // Neil: Create
    public async Task<AnnouncementDetailResponse> CreateAsync(
        Guid currentUserId,
        CreateAnnouncementRequest request,
        CancellationToken ct = default)
    {
        var categories = await GetCategoriesAsync(
            request.CategoryIds,
            ct);

        var now = DateTime.UtcNow;

        var announcement = new Announcement
        {
            Title = request.Title.Trim(),

            Summary =
                string.IsNullOrWhiteSpace(request.Summary)
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

        await _announcementRepository.AddAnnouncementAsync(
            announcement,
            ct);

        await _unitOfWork.SaveChangesAsync(ct);

        return MapToDetailResponse(announcement);
    }

    public async Task<AnnouncementDetailResponse> UpdateAsync(
        Guid currentUserId,
        Guid id,
        UpdateAnnouncementRequest request,
        CancellationToken ct = default)
    {
        var announcement = await _announcementRepository
            .GetForUpdateAsync(id, ct);

        if (announcement is null)
        {
            throw new NotFoundException(
                "Announcement not found.");
        }

        var categories = await GetCategoriesAsync(
            request.CategoryIds,
            ct);

        var now = DateTime.UtcNow;

        announcement.Title = request.Title.Trim();
        announcement.Summary = string.IsNullOrWhiteSpace(
            request.Summary)
                ? null
                : request.Summary.Trim();
        announcement.Content = request.Content.Trim();
        announcement.RoleReceived = request.RoleReceived;
        announcement.UpdatedBy = currentUserId;
        announcement.UpdatedAt = now;

        announcement.AnnouncementCategory.Clear();

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

        _announcementRepository.UpdateAnnouncement(
            announcement);

        await _unitOfWork.SaveChangesAsync(ct);

        return MapToDetailResponse(announcement);
    }

    public async Task PublishAsync(
        Guid currentUserId,
        Guid id,
        CancellationToken ct = default)
    {
        var announcement = await _announcementRepository
            .GetForUpdateAsync(id, ct);

        if (announcement is null)
        {
            throw new NotFoundException(
                "Announcement not found.");
        }

        if (announcement.Status != AnnouncementStatus.Draft)
        {
            throw new ConflictException(
                "Only draft announcements can be published.");
        }

        var now = DateTime.UtcNow;

        announcement.Status = AnnouncementStatus.Published;
        announcement.PublishedAt = now;
        announcement.UpdatedBy = currentUserId;
        announcement.UpdatedAt = now;

        _announcementRepository.UpdateAnnouncement(
            announcement);

        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task SoftDeleteAsync(
        Guid currentUserId,
        Guid id,
        CancellationToken ct = default)
    {
        var announcement = await _announcementRepository
            .GetForUpdateAsync(id, ct);

        if (announcement is null)
        {
            throw new NotFoundException(
                "Announcement not found.");
        }

        announcement.IsDeleted = true;
        announcement.UpdatedBy = currentUserId;
        announcement.UpdatedAt = DateTime.UtcNow;

        _announcementRepository.UpdateAnnouncement(
            announcement);

        await _unitOfWork.SaveChangesAsync(ct);
    }

    private static AnnouncementRoleReceived MapUserRole(
        string roleName)
    {
        return roleName
            .Trim()
            .ToLowerInvariant() switch
        {
            "student" =>
                AnnouncementRoleReceived.Student,

            "staff" =>
                AnnouncementRoleReceived.Staff,

            _ => throw new UnauthorizedAccessException(
                $"Role '{roleName}' cannot access announcements.")
        };
    }

    private async Task<IReadOnlyList<Category>> GetCategoriesAsync(
        IReadOnlyCollection<Guid> requestedCategoryIds,
        CancellationToken ct)
    {
        var categoryIds = requestedCategoryIds
            .Distinct()
            .ToList();

        var categories = await _categoryRepository.GetByIdsAsync(
            categoryIds,
            ct);

        if (categories.Count == categoryIds.Count)
        {
            return categories;
        }

        var existingCategoryIds = categories
            .Select(category => category.Id)
            .ToHashSet();

        var missingCategoryIds = categoryIds
            .Where(id => !existingCategoryIds.Contains(id));

        throw new BadRequestException(
            "Categories were not found: " +
            $"{string.Join(", ", missingCategoryIds)}.");
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
            PublishedAt = announcement.PublishedAt,
            CreatedAt = announcement.CreatedAt,

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
}
