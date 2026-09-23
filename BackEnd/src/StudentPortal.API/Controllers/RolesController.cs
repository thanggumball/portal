using Microsoft.AspNetCore.Mvc;
using StudentPortal.Common.DTOs.Role;
using StudentPortal.Common.DTOs.Shared;
using StudentPortal.Service.Interfaces;

namespace StudentPortal.API.Controllers;

// [Authorize]
[ApiController]
[Route("api/roles")]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _roleService.GetAllAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<RoleResponse>>.Ok(result));
    }
}
