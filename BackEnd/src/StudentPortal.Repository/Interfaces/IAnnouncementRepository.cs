using StudentPortal.Common.DTOs.Announcement;
using StudentPortal.Common.Enums;
using StudentPortal.Repository.Entities;

namespace StudentPortal.Repository.Interfaces;

public interface IAnnouncementRepository : IGenericRepository<Announcement>
{
    Task<(IReadOnlyList<Announcement> Items, int TotalCount)> SearchAsync(AnnouncementFilter filter, AnnouncementRoleReceived userRole, CancellationToken ct = default);
    Task<Announcement?> GetDetailedByIdAsync(

Guid announcementId,

AnnouncementRoleReceived userRole,
CancellationToken ct = default);
}
