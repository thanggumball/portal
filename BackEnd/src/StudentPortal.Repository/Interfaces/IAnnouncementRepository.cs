using StudentPortal.Common.DTOs.Announcement;
using StudentPortal.Repository.Entities;

namespace StudentPortal.Repository.Interfaces;

public interface IAnnouncementRepository : IGenericRepository<Announcement>
{
    Task<(IReadOnlyList<Announcement> Items, int Total)> SearchAsync(AnnouncementFilter filter, CancellationToken ct = default);
}
