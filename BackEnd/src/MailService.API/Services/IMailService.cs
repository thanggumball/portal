using MailService.API.DTOs;

namespace MailService.API.Services;

public interface IMailService
{
    Task<MailResponse> SendAsync(
        SendMailRequest request,
        CancellationToken ct = default);

    Task<List<MailResponse>> GetAllAsync(
        CancellationToken ct = default);

    Task<MailResponse?> GetByIdAsync(
        Guid id,
        CancellationToken ct = default);
}
