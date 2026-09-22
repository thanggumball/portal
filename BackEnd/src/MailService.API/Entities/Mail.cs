using MailService.API.Enums;

namespace MailService.API.Entities;

public class Mail
{
    public Guid Id { get; set; }

    public string? ThreadId { get; set; }

    public string MessageId { get; set; } = string.Empty;

    public string? InReplyTo { get; set; }

    public string? References { get; set; }

    public string From { get; set; } = string.Empty;

    public string To { get; set; } = string.Empty;

    public string? Cc { get; set; }

    public string? Bcc { get; set; }

    public string? ReplyTo { get; set; }

    public string Subject { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public bool IsHtml { get; set; }

    public MailDirection Direction { get; set; }

    public MailStatus Status { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime? SentAt { get; set; }

    public DateTime? ReceivedAt { get; set; }

    public DateTime CreatedAt { get; set; }
}
