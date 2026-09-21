using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentPortal.Repository.Entities;

namespace StudentPortal.Repository.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.Property(a => a.Id).HasDefaultValueSql("NEWSEQUENTIALID()").ValueGeneratedOnAdd();
        builder.Property(a => a.Action).HasColumnType("varchar(100)").IsRequired();
        builder.Property(a => a.EntityName).HasColumnType("varchar(100)").IsRequired();
        builder.Property(a => a.EntityId).HasColumnType("varchar(100)");
        builder.Property(a => a.IpAddress).HasColumnType("varchar(45)");
        builder.Property(a => a.OldValue).HasColumnType("nvarchar(max)");
        builder.Property(a => a.NewValue).HasColumnType("nvarchar(max)");

        // UserId nullable -> SetNull, so deleting a user doesn't lose history
        builder.HasOne(a => a.User).WithMany()
               .HasForeignKey(a => a.UserId).OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(a => new { a.UserId, a.CreatedAt })
               .IsDescending(false, true)
               .IncludeProperties(a => new { a.Action, a.EntityName, a.EntityId })
               .HasDatabaseName("IX_AuditLogs_UserId_CreatedAt");

        // Needs its own index: the index above does NOT serve a query that only
        // sorts by CreatedAt without filtering UserId (the overall audit log screen)
        builder.HasIndex(a => a.CreatedAt)
               .IsDescending(true)
               .IncludeProperties(a => new { a.UserId, a.Action, a.EntityName })
               .HasDatabaseName("IX_AuditLogs_CreatedAt");
    }
}
