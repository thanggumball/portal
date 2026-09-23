using StudentPortal.Common.DTOs.Mail;

namespace StudentPortal.Service.Interfaces;

public interface IMailServiceClient
{
    Task CreateAccountAsync(
        CreateMailAccountRequest request,
        CancellationToken ct = default);

    Task SendMailAsync(
        SendMailRequest request,
        CancellationToken ct = default);
}
