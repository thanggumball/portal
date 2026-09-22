using System.Security.Claims;

namespace MailService.API.Services;

public class CurrentMailUser : ICurrentMailUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentMailUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?
                .User
                .FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(value, out var id)
                ? id
                : Guid.Empty;
        }
    }

    public string Email =>
        _httpContextAccessor.HttpContext?
            .User
            .FindFirstValue(ClaimTypes.Email)
        ?? string.Empty;

    public string FullName =>
        _httpContextAccessor.HttpContext?
            .User
            .FindFirstValue(ClaimTypes.Name)
        ?? string.Empty;
}
