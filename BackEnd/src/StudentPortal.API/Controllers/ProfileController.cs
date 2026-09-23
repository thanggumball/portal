using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPortal.Common.DTOs.Profile;
using StudentPortal.Common.DTOs.Shared;
using StudentPortal.Common.DTOs.User;
using StudentPortal.Service.Implementations;
using StudentPortal.Service.Interfaces;
using System.Security.Claims;

namespace StudentPortal.API.Controllers;

[Authorize]
[ApiController]
[Route("api/profile")]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyProfile(
    CancellationToken ct)
    {
        var userId = Guid.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var result = await _profileService.GetMyProfileAsync(
            userId,
            ct);

        return StatusCode(
            StatusCodes.Status200OK,
            ApiResponse<ProfileResponse>.Ok(result));
    }

    [HttpPut]
    public Task<IActionResult> UpdateMyProfile(UpdateProfileRequest request, CancellationToken ct)
        => throw new NotImplementedException();
}
