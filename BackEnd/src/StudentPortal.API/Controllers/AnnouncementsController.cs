using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPortal.Common.Constants;
using StudentPortal.Common.DTOs.Announcement;


namespace StudentPortal.API.Controllers;

[Authorize]
[ApiController]
[Route("api/announcements")]
public class AnnouncementsController : ControllerBase
{
    [HttpGet]
    public Task<IActionResult> Search([FromQuery] AnnouncementFilter filter, CancellationToken ct)
        => throw new NotImplementedException();

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => throw new NotImplementedException();

    [Authorize(Roles = RoleConstants.Admin)]
    [HttpPost]
    public Task<IActionResult> Create(CreateAnnouncementRequest request, CancellationToken ct)
        => throw new NotImplementedException();

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
