using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentPortal.Common.Enums;
using StudentPortal.Repository.Entities;

namespace StudentPortal.Repository.Data.Configurations;

public class AnnouncementConfiguration : IEntityTypeConfiguration<Announcement>
{
    public void Configure(EntityTypeBuilder<Announcement> builder)
    {
        builder.Property(a => a.Id).HasDefaultValueSql("NEWSEQUENTIALID()").ValueGeneratedOnAdd();
        builder.Property(a => a.Title).HasMaxLength(255).IsRequired();
        builder.Property(a => a.Summary).HasMaxLength(1000);
        builder.Property(a => a.Content).HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(a => a.IsDeleted).HasDefaultValue(false);
        builder.Property(a => a.RoleReceived).HasDefaultValue(AnnouncementRoleReceived.All).IsRequired();

        // MUST be Restrict for both - two FKs pointing to Users would cause a
        // multiple cascade paths error if left at the default
        builder.HasOne(a => a.Creator).WithMany()
               .HasForeignKey(a => a.CreatedBy).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(a => a.Updater).WithMany()
               .HasForeignKey(a => a.UpdatedBy).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("GETDATE()");
        builder.HasIndex(a => new { a.Status, a.PublishedAt })
               .IsDescending(false, true)
               .IncludeProperties(a => new { a.Title, a.Summary })
               .HasFilter("[IsDeleted] = 0")
               .HasDatabaseName("IX_Announcements_Status_PublishedAt");
    }
}
