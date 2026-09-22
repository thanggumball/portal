using MailService.API.DTOs;
using MailService.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace MailService.API.Controllers;

[ApiController]
[Route("api/mail")]
public class MailController : ControllerBase
{
    private readonly IMailService _mailService;

    public MailController(IMailService mailService)
    {
        _mailService = mailService;
    }

    [HttpPost("send")]
    public async Task<IActionResult> Send(
        SendMailRequest request,
        CancellationToken ct)
    {
        var result = await _mailService.SendAsync(request, ct);

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken ct)
    {
        var result = await _mailService.GetAllAsync(ct);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken ct)
    {
        var result = await _mailService.GetByIdAsync(id, ct);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }
}
