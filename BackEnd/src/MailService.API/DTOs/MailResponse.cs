namespace MailService.API.DTOs;

public class MailResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
}
