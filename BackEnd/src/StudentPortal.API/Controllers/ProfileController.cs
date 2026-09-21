using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPortal.Common.DTOs.Profile;

namespace StudentPortal.API.Controllers;

[Authorize]
[ApiController]
[Route("api/profile")]
public class ProfileController : ControllerBase
{
    [HttpGet]
    public Task<IActionResult> GetMyProfile(CancellationToken ct)
        => throw new NotImplementedException();

    [HttpPut]
    public Task<IActionResult> UpdateMyProfile(UpdateProfileRequest request, CancellationToken ct)
        => throw new NotImplementedException();
}
