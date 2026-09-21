using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPortal.Common.Constants;
using StudentPortal.Common.DTOs.User;

namespace StudentPortal.API.Controllers;

[Authorize(Roles = RoleConstants.Admin)]
[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    [HttpGet]
    public Task<IActionResult> Search([FromQuery] UserFilter filter, CancellationToken ct)
        => throw new NotImplementedException();

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => throw new NotImplementedException();

    [HttpPost]
    public Task<IActionResult> Create(CreateUserRequest request, CancellationToken ct)
        => throw new NotImplementedException();

    [HttpPut("{id:guid}")]
    public Task<IActionResult> Update(Guid id, UpdateUserRequest request, CancellationToken ct)
        => throw new NotImplementedException();

    [HttpPatch("{id:guid}/status")]
    public Task<IActionResult> UpdateStatus(Guid id, UpdateUserStatusRequest request, CancellationToken ct)
        => throw new NotImplementedException();

    [HttpDelete("{id:guid}")]
    public Task<IActionResult> Delete(Guid id, CancellationToken ct)
        => throw new NotImplementedException();
}
