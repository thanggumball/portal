using MailService.API.DTOs;
using MailService.API.Entities;
using MailService.API.Enums;
using MailService.API.Repositories;

namespace MailService.API.Services;

public class MailService : IMailService
{
    private readonly IMailRepository _mailRepository;
    private readonly ICurrentMailUser _currentUser;

    public MailService(
        IMailRepository mailRepository,
        ICurrentMailUser currentUser)
    {
        _mailRepository = mailRepository;
        _currentUser = currentUser;
    }

    public async Task<MailResponse> SendAsync(
        SendMailRequest request,
        CancellationToken ct = default)
    {
        var from = _currentUser.Email;

        if (string.IsNullOrWhiteSpace(from))
        {
            throw new UnauthorizedAccessException(
                "User is not authenticated.");
        }

        var now = DateTime.UtcNow;

        var sentMail = new Mail
        {
            Id = Guid.NewGuid(),

            MessageId =
                $"<{Guid.NewGuid():N}@studentportal.local>",

            From = from,
            To = request.To,
            Cc = request.Cc,
            Bcc = request.Bcc,

            Subject = request.Subject,
            Body = request.Body,
            IsHtml = request.IsHtml,

            Direction = MailDirection.Sent,
            Status = MailStatus.Sent,

            SentAt = now,
            CreatedAt = now
        };

        var receivedMail = new Mail
        {
            Id = Guid.NewGuid(),

            MessageId =
                $"<{Guid.NewGuid():N}@studentportal.local>",

            From = from,
            To = request.To,
            Cc = request.Cc,
            Bcc = request.Bcc,

            Subject = request.Subject,
            Body = request.Body,
            IsHtml = request.IsHtml,

            Direction = MailDirection.Received,
            Status = MailStatus.Received,

            SentAt = now,
            ReceivedAt = now,
            CreatedAt = now
        };

        await _mailRepository.AddAsync(
            sentMail,
            ct);

        await _mailRepository.AddAsync(
            receivedMail,
            ct);

        await _mailRepository.SaveChangesAsync(ct);

        return MapToResponse(sentMail);
    }

    public async Task<List<MailResponse>> GetInboxAsync(
        CancellationToken ct = default)
    {
        var email = _currentUser.Email;

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new UnauthorizedAccessException(
                "User is not authenticated.");
        }

        var mails = await _mailRepository.GetInboxAsync(
            email,
            ct);

        return mails
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<List<MailResponse>> GetSentAsync(
        CancellationToken ct = default)
    {
        var email = _currentUser.Email;

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new UnauthorizedAccessException(
                "User is not authenticated.");
        }

        var mails = await _mailRepository.GetSentAsync(
            email,
            ct);

        return mails
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<MailResponse?> GetByIdAsync(
        Guid id,
        CancellationToken ct = default)
    {
        var email = _currentUser.Email;

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new UnauthorizedAccessException(
                "User is not authenticated.");
        }

        var mail = await _mailRepository.GetByIdAsync(
            id,
            ct);

        if (mail is null)
        {
            return null;
        }

        var canAccess =
            mail.From == email ||
            mail.To == email;

        if (!canAccess)
        {
            return null;
        }

        return MapToResponse(mail);
    }

    private static MailResponse MapToResponse(Mail mail)
    {
        return new MailResponse
        {
            Id = mail.Id,
            MessageId = mail.MessageId,
            ThreadId = mail.ThreadId,
            InReplyTo = mail.InReplyTo,

            From = mail.From,
            To = mail.To,
            Cc = mail.Cc,
            Bcc = mail.Bcc,

            Subject = mail.Subject,
            Body = mail.Body,
            IsHtml = mail.IsHtml,

            SentAt = mail.SentAt,
            ReceivedAt = mail.ReceivedAt,
            CreatedAt = mail.CreatedAt
        };
    }
}
