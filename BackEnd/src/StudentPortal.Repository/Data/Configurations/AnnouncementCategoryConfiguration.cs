using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentPortal.Repository.Entities;

namespace StudentPortal.Repository.Data.Configurations;

public class AnnouncementCategoryConfiguration
    : IEntityTypeConfiguration<AnnouncementCategory>
{
    public void Configure(EntityTypeBuilder<AnnouncementCategory> builder)
    {
        builder.Property(ac => ac.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()")
            .ValueGeneratedOnAdd();

        builder.HasOne(ac => ac.Announcement)
            .WithMany(a => a.AnnouncementCategory)
            .HasForeignKey(ac => ac.AnnouncementId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ac => ac.Category)
            .WithMany(c => c.AnnouncementCategory)
            .HasForeignKey(ac => ac.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(ac => new
        {
            ac.AnnouncementId,
            ac.CategoryId
        }).IsUnique();

        builder.ToTable("AnnouncementCategory");
    }
}
