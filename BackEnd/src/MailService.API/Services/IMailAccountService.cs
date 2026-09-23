using MailService.API.DTOs;

namespace MailService.API.Services;

public interface IMailAccountService
{
    Task<LoginResponse> CreateAsync(
        CreateAccountRequest request,
        CancellationToken ct = default);

    Task<LoginResponse?> LoginAsync(
        LoginRequest request,
        CancellationToken ct = default);
}
