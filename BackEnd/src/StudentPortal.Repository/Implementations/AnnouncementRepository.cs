using Microsoft.EntityFrameworkCore;
using StudentPortal.Common.DTOs.Announcement;
using StudentPortal.Common.Enums;
using StudentPortal.Repository.Data;
using StudentPortal.Repository.Entities;
using StudentPortal.Repository.Interfaces;

namespace StudentPortal.Repository.Implementations;

public class AnnouncementRepository
    : GenericRepository<Announcement>, IAnnouncementRepository
{
    public AnnouncementRepository(AppDbContext context)
        : base(context)
    {
    }

    public async Task<(IReadOnlyList<Announcement> Items, int TotalCount)>
        SearchAsync(
            AnnouncementFilter filter,
            AnnouncementRoleReceived userRole,
            CancellationToken ct = default)
    {
        var pageNumber = Math.Max(filter.Page, 1);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);

        IQueryable<Announcement> query = _context.Announcements
            .AsNoTracking()
            .Where(a =>
                !a.IsDeleted &&
                a.Status == AnnouncementStatus.Published &&
                a.CreatedAt <= DateTime.UtcNow &&
                (
                    a.RoleReceived == AnnouncementRoleReceived.All ||
                    a.RoleReceived <= userRole
                ));

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            var keyword = filter.Keyword.Trim();

            query = query.Where(a =>
                a.Title.Contains(keyword) ||
                (a.Summary != null && a.Summary.Contains(keyword)) ||
                a.Content.Contains(keyword));
        }

        if (!string.IsNullOrWhiteSpace(filter.CategoryName))
        {
            var categoryName = filter.CategoryName.Trim();

            query = query.Where(a =>
                a.AnnouncementCategory.Any(ac =>
                    ac.Category.Name == categoryName));
        }

        if (filter.StartDate.HasValue)
        {
            var publishedFrom = filter.StartDate.Value.Date;

            query = query.Where(a =>
                a.PublishedAt >= publishedFrom);
        }

        if (filter.EndDate.HasValue)
        {
            var publishedToExclusive = filter.EndDate
                .Value
                .Date
                .AddDays(1);

            query = query.Where(a =>
                a.PublishedAt < publishedToExclusive);
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Include(a => a.AnnouncementCategory)
                .ThenInclude(ac => ac.Category)
            .OrderByDescending(a => a.PublishedAt)
            .ThenByDescending(a => a.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }
    public async Task<Announcement?> GetDetailedByIdAsync(
        Guid announcementId,
        AnnouncementRoleReceived userRole,
        CancellationToken ct = default)
    {
        return await _context.Announcements
            .AsNoTracking()
            .Include(a => a.AnnouncementCategory)
                .ThenInclude(ac => ac.Category)
            .FirstOrDefaultAsync(
                a =>
                    a.Id == announcementId &&
                    !a.IsDeleted &&
                    a.Status == AnnouncementStatus.Published &&
                    a.CreatedAt <= DateTime.UtcNow &&
                    a.RoleReceived <= userRole,
                ct);
    }
}

