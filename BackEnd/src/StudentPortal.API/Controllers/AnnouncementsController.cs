using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPortal.Common.Constants;
using StudentPortal.Common.DTOs.Announcement;
using StudentPortal.Common.DTOs.Shared;
using StudentPortal.Service.Interfaces;
using System.Security.Claims;

namespace StudentPortal.API.Controllers;

[Authorize]
[ApiController]
[Route("api/announcements")]
public class AnnouncementsController : ControllerBase
{
    private readonly IAnnouncementService _announcementService;
    public AnnouncementsController(
        IAnnouncementService announcementService)
    {
        _announcementService = announcementService;
    }
    [Authorize(Roles = RoleConstants.Admin)]
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] AnnouncementFilter filter, CancellationToken ct)
    {
        var result = await _announcementService.SearchAsync(
            filter,
            ct);

        return Ok(
            ApiResponse<PagedResult<AnnouncementResponse>>
                .Ok(result));
    }
    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => throw new NotImplementedException();

    [Authorize(Roles = RoleConstants.Admin)]
    [HttpPost]
    public async Task<IActionResult> Create(CreateAnnouncementRequest request, CancellationToken ct)
    {
        var currentUserId = Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var result = await _announcementService.CreateAsync(
            currentUserId,
            request,
            ct);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            ApiResponse<AnnouncementDetailResponse>.Ok(result));
    }

    [Authorize(Roles = RoleConstants.Admin)]
    [HttpPut("{id:guid}")]
    public Task<IActionResult> Update(Guid id, UpdateAnnouncementRequest request, CancellationToken ct)
        => throw new NotImplementedException();

    [Authorize(Roles = RoleConstants.Admin)]
    [HttpPatch("{id:guid}/publish")]
    public Task<IActionResult> Publish(Guid id, CancellationToken ct)
        => throw new NotImplementedException();

    [Authorize(Roles = RoleConstants.Admin)]
    [HttpDelete("{id:guid}")]
    public Task<IActionResult> Delete(Guid id, CancellationToken ct)
        => throw new NotImplementedException();
}
