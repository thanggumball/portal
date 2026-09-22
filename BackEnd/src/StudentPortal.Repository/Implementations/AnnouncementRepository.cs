using Microsoft.EntityFrameworkCore;
using StudentPortal.Common.DTOs.Announcement;
using StudentPortal.Repository.Data;
using StudentPortal.Repository.Entities;
using StudentPortal.Repository.Interfaces;

namespace StudentPortal.Repository.Implementations;

public class AnnouncementRepository : GenericRepository<Announcement>, IAnnouncementRepository
{
    public AnnouncementRepository(AppDbContext context) : base(context) { }

    public async Task<(IReadOnlyList<Announcement> Items, int Total)> SearchAsync(
        AnnouncementFilter filter, CancellationToken ct = default)
    {
        var query = _context.Announcements
            .AsNoTracking()
            .Where(announcement => !announcement.IsDeleted);

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            var keyword = filter.Keyword.Trim();

            query = query.Where(announcement =>
                announcement.Title.Contains(keyword) ||
                (
                    announcement.Summary != null &&
                    announcement.Summary.Contains(keyword)
                ));
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(announcement =>
                announcement.Status == filter.Status.Value);
        }

        if (filter.RoleReceived.HasValue)
        {
            query = query.Where(announcement =>
                announcement.RoleReceived ==
                filter.RoleReceived.Value);
        }

        if (filter.CategoryId.HasValue)
        {
            query = query.Where(announcement =>
                announcement.AnnouncementCategory.Any(link =>
                    link.CategoryId == filter.CategoryId.Value));
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .Include(announcement => announcement.AnnouncementCategory)
            .ThenInclude(link => link.Category)
            .OrderByDescending(announcement => announcement.CreatedAt)
            .ThenBy(announcement => announcement.Id)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        return (items, total);

    }

    public async Task<IEnumerable<Announcement>> GetAnnouncementsAsync(CancellationToken cancellationToken)
    {
        var resultSet = await _context.Announcements.OrderByDescending(a => a.CreatedAt).ToListAsync();
        return resultSet;
    }

}
