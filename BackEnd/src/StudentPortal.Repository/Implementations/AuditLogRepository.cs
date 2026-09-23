using Microsoft.EntityFrameworkCore;
using StudentPortal.Common.DTOs.AuditLog;
using StudentPortal.Repository.Data;
using StudentPortal.Repository.Entities;
using StudentPortal.Repository.Interfaces;

namespace StudentPortal.Repository.Implementations;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly AppDbContext _context;

    public AuditLogRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(AuditLog log, CancellationToken ct = default)
        => await _context.AuditLogs.AddAsync(log, ct);

    public async Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.AuditLogs
            .AsNoTracking()
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<(IReadOnlyList<AuditLogListItem> Items, int Total)> SearchAsync(
        AuditLogFilter f,
        CancellationToken ct = default)
    {
        // Page and PageSize are already clamped by PagingRequest
        var query = _context.AuditLogs.AsNoTracking();

        if (f.UserId.HasValue)
            query = query.Where(a => a.UserId == f.UserId.Value);

        if (!string.IsNullOrWhiteSpace(f.EntityName))
            query = query.Where(a => a.EntityName == f.EntityName);

        if (!string.IsNullOrWhiteSpace(f.Action))
            query = query.Where(a => a.Action == f.Action);

        if (f.From.HasValue)
            query = query.Where(a => a.CreatedAt >= f.From.Value);

        if (f.To.HasValue)
            query = query.Where(a => a.CreatedAt < f.To.Value);

        var total = await query.CountAsync(ct);

        // The list deliberately does NOT pull OldValue / NewValue - both are nvarchar(max).
        // Only the detail screen fetches them.
        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((f.Page - 1) * f.PageSize)
            .Take(f.PageSize)
            .Select(a => new AuditLogListItem
            {
                Id = a.Id,
                UserId = a.UserId,
                UserName = a.User != null ? a.User.UserName : null,
                Action = a.Action,
                EntityName = a.EntityName,
                EntityId = a.EntityId,
                IpAddress = a.IpAddress,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync(ct);

        return (items, total);
    }
}
