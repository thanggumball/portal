using Audit.EntityFramework.Providers;
using Microsoft.EntityFrameworkCore;
using StudentPortal.Repository.Data;
using StudentPortal.Repository.Entities;

namespace StudentPortal.Repository.Auditing;

public static class AuditConfiguration
{
    /// <summary>Global Audit.NET setup. Call exactly ONCE at startup.</summary>
    /// <param name="onAuditError">
    /// Where to report an audit row that could not be written. Defaults to stderr.
    /// </param>
    public static void Configure(
        string connectionString,
        Action<Audit.Core.AuditEvent, Exception>? onAuditError = null)
    {
        // ---- 1. Which tables are audited, which columns are dropped ----
        Audit.EntityFramework.Configuration.Setup()
            .ForContext<AppDbContext>(config => config
                .ForEntity<User>(user => user
                    .Ignore(u => u.PasswordHash)     // sensitive
                    .Ignore(u => u.LastLoginAt)))    // changes on every login, all noise
            .UseOptIn()                              // audit NOTHING except the types listed below
                .Include<User>()
                .Include<Announcement>()
                .Include<Role>()
                .Include<EmailWhitelist>();

        // Deliberately NOT included:
        //   AuditLog     - auditing itself would loop forever
        //   RefreshToken - churns constantly, would flood the table
        //   UserMfa      - SecretKey is sensitive

        // ---- 2. Where the events are written ----
        // A SEPARATE DbContext with NO AuditSaveChangesInterceptor attached.
        // That way writing an AuditLog does not trigger auditing again, and it does not
        // interfere with the DbContext currently serving the request.
        var auditDbOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        var efDataProvider = new EntityFrameworkDataProvider
        {
            DbContextBuilder = _ => new AppDbContext(auditDbOptions),
            DisposeDbContext = true,

            // Every audited table lands in the SINGLE AuditLogs table
            AuditTypeMapper = (_, _) => typeof(AuditLog),

            // REQUIRED. By default the library copies same-named properties from the source
            // entity onto the audit entity - User.Id would overwrite AuditLog.Id and collide
            // on the primary key.
            IgnoreMatchedPropertiesFunc = _ => true,

            AuditEntityAction = (auditEvent, entry, auditEntity) =>
                Task.FromResult(AuditLogMapper.TryMap(auditEvent, entry, (AuditLog)auditEntity))
        };

        // ---- 3. Keep audit failures out of the business request ----
        // The audit row is written from inside the caller's SaveChangesAsync. Unwrapped, an
        // exception there surfaces as a 500 even though the business data was committed.
        Audit.Core.Configuration.DataProvider =
            new SafeAuditDataProvider(efDataProvider, onAuditError);
    }
}
