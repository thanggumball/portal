using MailService.API.Data;
using MailService.API.Entities;
using MailService.API.Enums;
using Microsoft.EntityFrameworkCore;

namespace MailService.API.Repositories;

public class MailRepository : IMailRepository
{
    private readonly MailDbContext _context;

    public MailRepository(MailDbContext context)
    {
        _context = context;
    }

    public async Task<Mail?> GetByIdAsync(
        Guid id,
        CancellationToken ct = default)
    {
        return await _context.Mails
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<Mail?> GetByMessageIdAsync(
        string messageId,
        CancellationToken ct = default)
    {
        return await _context.Mails
            .FirstOrDefaultAsync(x => x.MessageId == messageId, ct);
    }

    public async Task<List<Mail>> GetInboxAsync(
        string email,
        CancellationToken ct = default)
    {
        return await _context.Mails
            .Where(x =>
                x.To == email &&
                x.Direction == MailDirection.Received)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<List<Mail>> GetSentAsync(
        string email,
        CancellationToken ct = default)
    {
        return await _context.Mails
            .Where(x =>
                x.From == email &&
                x.Direction == MailDirection.Sent)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task AddAsync(
        Mail mail,
        CancellationToken ct = default)
    {
        await _context.Mails.AddAsync(mail, ct);
    }

    public async Task SaveChangesAsync(
        CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
}

