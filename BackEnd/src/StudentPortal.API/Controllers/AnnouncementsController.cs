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
    [HttpPost]
    public async Task<IActionResult> Create(
        CreateAnnouncementRequest request,
        CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();

        var result = await _announcementService.CreateAsync(
            currentUserId,
            request,
            ct);

        return CreatedAtAction(
            nameof(GetByIdAsync),
            new { id = result.Id },
            ApiResponse<AnnouncementDetailResponse>.Ok(result));
    }

    [Authorize(Roles = RoleConstants.Admin)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateAnnouncementRequest request,
        CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();

        var result = await _announcementService.UpdateAsync(
            currentUserId,
            id,
            request,
            ct);

        return Ok(
            ApiResponse<AnnouncementDetailResponse>.Ok(result));
    }

    [Authorize(Roles = RoleConstants.Admin)]
    [HttpPatch("{id:guid}/publish")]
    public async Task<IActionResult> Publish(
        Guid id,
        CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();

        await _announcementService.PublishAsync(
            currentUserId,
            id,
            ct);

        return NoContent();
    }

    [Authorize(Roles = RoleConstants.Admin)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();

        await _announcementService.SoftDeleteAsync(
            currentUserId,
            id,
            ct);

        return NoContent();
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException(
                "User ID claim is missing or invalid.");
        }

        return userId;
    }
}
