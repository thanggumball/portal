using MailService.API.DTOs;
using MailService.API.Entities;
using MailService.API.Enums;
using MailService.API.Repositories;

namespace MailService.API.Services;

public class MailService : IMailService
{
    private readonly IMailRepository _mailRepository;

    public MailService(IMailRepository mailRepository)
    {
        _mailRepository = mailRepository;
    }

    public async Task<MailResponse> SendAsync(
        SendMailRequest request,
        CancellationToken ct = default)
    {
        var mail = new Mail
        {
            Id = Guid.NewGuid(),
            MessageId = $"<{Guid.NewGuid():N}@studentportal.local>",

            From = request.To,
            To = request.To,

            Subject = request.Subject,
            Body = request.Body,
            IsHtml = request.IsHtml,

            Direction = MailDirection.Sent,
            Status = MailStatus.Pending,

            CreatedAt = DateTime.UtcNow
        };

        await _mailRepository.AddAsync(mail, ct);
        await _mailRepository.SaveChangesAsync(ct);

        return new MailResponse
        {
            Success = true,
            Message = "Email saved successfully."
        };
    }

    public async Task<List<MailResponse>> GetAllAsync(
        CancellationToken ct = default)
    {
        var mails = await _mailRepository.GetAllAsync(ct);

        return mails.Select(mail => new MailResponse
        {
            Success = true,
            Message = $"{mail.Subject} - {mail.Status}"
        }).ToList();
    }

    public async Task<MailResponse?> GetByIdAsync(
        Guid id,
        CancellationToken ct = default)
    {
        var mail = await _mailRepository.GetByIdAsync(id, ct);

        if (mail is null)
        {
            return null;
        }

        return new MailResponse
        {
            Success = true,
            Message = $"{mail.Subject} - {mail.Status}"
        };
    }
}
