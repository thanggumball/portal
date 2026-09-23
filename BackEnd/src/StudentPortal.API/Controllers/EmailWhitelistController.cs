using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPortal.Common.Constants;
using StudentPortal.Common.DTOs.EmailWhitelist;
using StudentPortal.Common.DTOs.Shared;
using StudentPortal.Service.Interfaces;

namespace StudentPortal.API.Controllers;

[Authorize(Roles = RoleConstants.Admin)]
[ApiController]
[Route("api/email-whitelist")]
public class EmailWhitelistController : ControllerBase
{
    private readonly IEmailWhitelistService _emailWhitelistService;

    public EmailWhitelistController(
        IEmailWhitelistService emailWhitelistService)
    {
        _emailWhitelistService = emailWhitelistService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateEmailWhitelistRequest request,
        CancellationToken ct)
    {
        var userId = Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        await _emailWhitelistService.CreateAsync(
            request,
            userId,
            ct);

        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<object?>.Ok(
                null,
                "Email added to whitelist."));
    }
}