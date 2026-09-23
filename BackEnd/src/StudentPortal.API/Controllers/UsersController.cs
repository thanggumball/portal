using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPortal.Common.Constants;
using StudentPortal.Common.DTOs.Shared;
using StudentPortal.Common.DTOs.User;
using StudentPortal.Service.Interfaces;

namespace StudentPortal.API.Controllers;

// [Authorize(Roles = RoleConstants.Admin)]
[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] UserFilter filter,
        CancellationToken ct)
    {
        var result = await _userService.SearchUsersAsync(filter, ct);
        return Ok(ApiResponse<PagedResult<UserResponse>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetById(
        Guid id,
        CancellationToken ct)
        => throw new NotImplementedException();

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateUserRequest request,
        CancellationToken ct)
    {
        var result = await _userService.CreateUserAsync(
            request,
            ct);

        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<UserResponse>.Ok(result));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateUserRequest request,
        CancellationToken ct)
    {
        var result = await _userService.UpdateUserAsync(id, request, ct);

        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<UserResponse>.Ok(result));
    }

    [HttpPatch("{id:guid}/status")]
    public Task<IActionResult> UpdateStatus(
        Guid id,
        UpdateUserStatusRequest request,
        CancellationToken ct)
        => throw new NotImplementedException();

    [HttpDelete("{id:guid}")]
    public Task<IActionResult> Delete(
        Guid id,
        CancellationToken ct)
        => throw new NotImplementedException();
}
