using MailService.API.DTOs;
using MailService.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MailService.API.Controllers;

[ApiController]
[Route("api/mail")]
[Authorize]
public class MailController : ControllerBase
{
    private readonly IMailService _mailService;

    public MailController(IMailService mailService)
    {
        _mailService = mailService;
    }

    [HttpPost]
    public async Task<IActionResult> Send(
        SendMailRequest request,
        CancellationToken ct)
    {
        try
        {
            var result = await _mailService.SendAsync(
                request,
                ct);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new
            {
                message = ex.Message
            });
        }
    }

    [HttpGet("inbox")]
    public async Task<IActionResult> Inbox(
        CancellationToken ct)
    {
        try
        {
            var result = await _mailService.GetInboxAsync(ct);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new
            {
                message = ex.Message
            });
        }
    }

    [HttpGet("sent")]
    public async Task<IActionResult> Sent(
        CancellationToken ct)
    {
        try
        {
            var result = await _mailService.GetSentAsync(ct);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new
            {
                message = ex.Message
            });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken ct)
    {
        try
        {
            var result = await _mailService.GetByIdAsync(
                id,
                ct);

            if (result is null)
            {
                return NotFound(new
                {
                    message = "Mail not found."
                });
            }

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new
            {
                message = ex.Message
            });
        }
    }

    [AllowAnonymous]
    [HttpPost("internal/send")]
    public async Task<IActionResult> SendInternal(
    InternalSendMailRequest request,
    CancellationToken ct)
    {
        var result = await _mailService.SendInternalAsync(
            request,
            ct);

        return Ok(result);
    }
}

