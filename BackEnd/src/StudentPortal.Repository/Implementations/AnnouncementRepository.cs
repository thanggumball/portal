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
        var query = _context.Set<Announcement>().AsNoTracking();

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(a => a.CreatedAt) 
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<IEnumerable<Announcement>> GetAnnouncementsAsync()
    {
        var resultSet = await _context.Announcements.Get
    }
}
