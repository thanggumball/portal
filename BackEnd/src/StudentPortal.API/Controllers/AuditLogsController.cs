using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPortal.Common.Constants;
using StudentPortal.Common.DTOs.AuditLog;
using StudentPortal.Common.DTOs.Shared;
using StudentPortal.Service.Interfaces;

namespace StudentPortal.API.Controllers;

[Authorize(Roles = RoleConstants.Admin)]
[ApiController]
[Route("api/audit-logs")]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogsController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] AuditLogFilter filter,
        CancellationToken ct)
    {
        var result = await _auditLogService.SearchAsync(filter, ct);

        return Ok(ApiResponse<PagedResult<AuditLogListItem>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken ct)
    {
        var result = await _auditLogService.GetByIdAsync(id, ct);

        return Ok(ApiResponse<AuditLogDetail>.Ok(result));
    }

    // Deliberately no POST, PUT or DELETE - audit logs are read-only
}
