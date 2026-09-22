using BCrypt.Net;
using MailService.API.DTOs;
using MailService.API.Entities;
using MailService.API.Repositories;

namespace MailService.API.Services;

public class MailAccountService : IMailAccountService
{
    private readonly IMailAccountRepository _repository;

    public MailAccountService(IMailAccountRepository repository)
    {
        _repository = repository;
    }

    public async Task<LoginResponse> CreateAsync(
        CreateAccountRequest request,
        CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var existingAccount = await _repository.GetByEmailAsync(email, ct);

        if (existingAccount is not null)
        {
            throw new InvalidOperationException(
                "Mail account already exists.");
        }

        var account = new MailAccount
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = request.FullName.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(account, ct);
        await _repository.SaveChangesAsync(ct);

        return new LoginResponse
        {
            Id = account.Id,
            Email = account.Email,
            FullName = account.FullName
        };
    }

    public async Task<LoginResponse?> LoginAsync(
        LoginRequest request,
        CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var account = await _repository.GetByEmailAsync(email, ct);

        if (account is null || !account.IsActive)
        {
            return null;
        }

        var validPassword = BCrypt.Net.BCrypt.Verify(
            request.Password,
            account.PasswordHash);

        if (!validPassword)
        {
            return null;
        }

        return new LoginResponse
        {
            Id = account.Id,
            Email = account.Email,
            FullName = account.FullName
        };
    }
}
