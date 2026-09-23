using Audit.Core;

namespace StudentPortal.Repository.Auditing;

/// <summary>
/// Wraps another data provider so a failed audit write can never break the business
/// operation that triggered it.
///
/// Without this, Audit.NET writes the audit row from inside
/// <c>AuditSaveChangesInterceptor.SavedChangesAsync</c> - which runs INSIDE the caller's
/// <c>SaveChangesAsync</c>. The business data is already committed at that point (the audit
/// uses its own DbContext, hence its own transaction), but an exception thrown there still
/// propagates out to the caller. The result is the worst of both worlds: the data IS saved
/// and the API still returns 500.
///
/// Real triggers seen in testing: <c>AuditLog.UserId</c> carrying a user id that is not in
/// the Users table (FK violation), plus any transient fault on the audit connection.
/// </summary>
public class SafeAuditDataProvider : AuditDataProvider
{
    private readonly AuditDataProvider _inner;
    private readonly Action<AuditEvent, Exception> _onError;

    public SafeAuditDataProvider(
        AuditDataProvider inner,
        Action<AuditEvent, Exception>? onError = null)
    {
        _inner = inner;
        _onError = onError ?? DefaultOnError;
    }

    public override object InsertEvent(AuditEvent auditEvent)
    {
        try
        {
            return _inner.InsertEvent(auditEvent);
        }
        catch (Exception ex)
        {
            _onError(auditEvent, ex);
            return null!;
        }
    }

    public override async Task<object> InsertEventAsync(
        AuditEvent auditEvent,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _inner.InsertEventAsync(auditEvent, cancellationToken);
        }
        catch (Exception ex)
        {
            _onError(auditEvent, ex);
            return null!;
        }
    }

    public override void ReplaceEvent(object eventId, AuditEvent auditEvent)
    {
        try
        {
            _inner.ReplaceEvent(eventId, auditEvent);
        }
        catch (Exception ex)
        {
            _onError(auditEvent, ex);
        }
    }

    public override async Task ReplaceEventAsync(
        object eventId,
        AuditEvent auditEvent,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _inner.ReplaceEventAsync(eventId, auditEvent, cancellationToken);
        }
        catch (Exception ex)
        {
            _onError(auditEvent, ex);
        }
    }

    // A dropped audit row must never be silent. The Repository layer has no logger of its
    // own, so the default sink writes to stderr; pass a real logger from the composition
    // root to route this into log4net.
    private static void DefaultOnError(AuditEvent auditEvent, Exception ex) =>
        Console.Error.WriteLine(
            $"[AUDIT WRITE FAILED] {auditEvent.EventType}: {ex.GetBaseException().Message}");
}
