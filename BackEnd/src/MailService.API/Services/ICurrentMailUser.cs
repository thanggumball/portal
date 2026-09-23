namespace MailService.API.Services;

public interface ICurrentMailUser
{
    Guid UserId { get; }

    string Email { get; }

    string FullName { get; }
}
