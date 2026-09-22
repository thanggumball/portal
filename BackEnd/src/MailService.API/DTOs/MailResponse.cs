namespace MailService.API.DTOs;

public class MailResponse
{
    public Guid Id { get; set; }

    public string MessageId { get; set; } = string.Empty;

    public string? ThreadId { get; set; }

    public string? InReplyTo { get; set; }

    public string From { get; set; } = string.Empty;

    public string To { get; set; } = string.Empty;

    public string? Cc { get; set; }

    public string? Bcc { get; set; }

    public string Subject { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public bool IsHtml { get; set; }

    public DateTime? SentAt { get; set; }

    public DateTime? ReceivedAt { get; set; }

    public DateTime CreatedAt { get; set; }
}
