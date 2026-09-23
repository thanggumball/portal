using StudentPortal.Common.DTOs.Announcement;
using StudentPortal.Common.DTOs.Category;
using StudentPortal.Common.DTOs.Shared;
using StudentPortal.Common.Enums;
using StudentPortal.Repository.Entities;
using StudentPortal.Repository.Implementations;
using StudentPortal.Repository.Interfaces;
using StudentPortal.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace StudentPortal.Service.Implementations
{
    public class AnnouncementService : IAnnouncementService
    {
        private readonly IAnnouncementRepository _announcementRepository;
        private readonly IUserRepository _userRepository;
        public AnnouncementService(IAnnouncementRepository announcementRepository, IUserRepository userRepository)
        {
            _announcementRepository = announcementRepository;
            _userRepository = userRepository;
        }
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
                throw new KeyNotFoundException("User not found.");
            }

            if (user.Role is null)
            {
                throw new InvalidOperationException(
                    "The user has not been assigned a role.");
            }

            var userRole = MapUserRole(user.Role.Name);

            var announcement =
                await _announcementRepository.GetDetailedByIdAsync(
                    announcementId,
                    userRole,
                    ct);

            if (announcement is null)
            {
                throw new KeyNotFoundException(
                    "Announcement not found.");
            }

            return MapToDetailResponse(announcement);
        }
        public async Task<PagedResult<AnnouncementResponse>>
        SearchAsync(
            AnnouncementFilter filterRequest,
            Guid userId,
            CancellationToken ct)
        {
            var user = await _userRepository.GetByIdWithRoleAsync(userId);

            var userRole = MapUserRole(user.Role.Name);

            var (items, total) =
                    await _announcementRepository.SearchAsync(
                        filterRequest,
                        userRole,
                        ct);

            return new PagedResult<AnnouncementResponse>
            {
                Items = items.Select(MapToResponse).ToList(),
                Total = total,
                Page = filterRequest.Page,
                PageSize = filterRequest.PageSize
            };
        }

        private static AnnouncementRoleReceived MapUserRole(string roleName)
        {
            return roleName.Trim().ToLowerInvariant() switch
            {
                "student" => AnnouncementRoleReceived.Student,
                "staff" => AnnouncementRoleReceived.Staff,

                _ => throw new UnauthorizedAccessException(
                    $"Role '{roleName}' cannot access announcements.")
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
                PublishedAt = announcement.PublishedAt,
                CreatedAt = announcement.CreatedAt,

                Categories = announcement.AnnouncementCategory
                    .Select(ac => new CategoryResponse
                    {
                        Id = ac.CategoryId,
                        Name = ac.Category.Name
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
                PublishedAt = announcement.PublishedAt,
                CreatedAt = announcement.CreatedAt,

                Categories = announcement.AnnouncementCategory
                    .Select(ac => new CategoryResponse
                    {
                        Id = ac.CategoryId,
                        Name = ac.Category.Name
                    })
                    .ToList()
            };
        }
    }
}
