using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentPortal.Repository.Entities;

namespace StudentPortal.Repository.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.Property(c => c.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()")
            .ValueGeneratedOnAdd();

        builder.Property(c => c.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(c => c.Name)
            .IsUnique();

        builder.HasMany(c => c.AnnouncementCategory)
            .WithOne(ac => ac.Category)
            .HasForeignKey(ac => ac.CategoryId);
        builder.Property(x => x.CreatedAt)
    .HasDefaultValueSql("GETDATE()");
        builder.ToTable("Categories");
    }
}
