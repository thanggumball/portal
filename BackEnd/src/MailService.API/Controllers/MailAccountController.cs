using MailService.API.DTOs;
using MailService.API.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MailService.API.Controllers;

[ApiController]
[Route("api/mail-account")]
public class MailAccountController : ControllerBase
{
    private readonly IMailAccountService _service;

    public MailAccountController(IMailAccountService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateAccountRequest request,
        CancellationToken ct)
    {
        try
        {
            var result = await _service.CreateAsync(request, ct);

            return StatusCode(
                StatusCodes.Status201Created,
                result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        LoginRequest request,
        CancellationToken ct)
    {
        var result = await _service.LoginAsync(request, ct);

        if (result is null)
        {
            return Unauthorized(new
            {
                message = "Invalid email or password."
            });
        }

        var claims = new List<Claim>
        {
            new(
                ClaimTypes.NameIdentifier,
                result.Id.ToString()),

            new(
                ClaimTypes.Email,
                result.Email),

            new(
                ClaimTypes.Name,
                result.FullName)
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal);

        return Ok(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        return Ok(new
        {
            message = "Logged out successfully."
        });
    }
}

