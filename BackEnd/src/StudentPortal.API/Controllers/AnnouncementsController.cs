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
    public AnnouncementsController(IAnnouncementService announcementService)
    {
        _announcementService = announcementService;
    }
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> SearchAsync(
        [FromQuery] AnnouncementFilter filter,
        CancellationToken ct)
    {
        Guid? userId = null;

        var userIdClaim =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (Guid.TryParse(userIdClaim, out var parsedUserId))
        {
            userId = parsedUserId;
        }

        var result = await _announcementService.SearchAsync(
            filter,
            userId,
            ct);

        return Ok(result);
    }
    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetByIdAsync(
    Guid id,
    CancellationToken ct)
    {
        Guid? userId = null;

        var userIdClaim =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (Guid.TryParse(userIdClaim, out var parsedUserId))
        {
            userId = parsedUserId;
        }

        var result = await _announcementService.GetByIdAsync(
            id,
            userId,
            ct);

        return Ok(result);
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
