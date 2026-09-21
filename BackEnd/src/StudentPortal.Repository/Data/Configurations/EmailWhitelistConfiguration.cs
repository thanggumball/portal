using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentPortal.Repository.Entities;

namespace StudentPortal.Repository.Data.Configurations;

public class EmailWhitelistConfiguration : IEntityTypeConfiguration<EmailWhitelist>
{
    public void Configure(EntityTypeBuilder<EmailWhitelist> builder)
    {
        builder.Property(e => e.Id).HasDefaultValueSql("NEWSEQUENTIALID()").ValueGeneratedOnAdd();
        builder.Property(e => e.Domain).HasColumnType("varchar(255)").IsRequired();
        builder.Property(e => e.Note).HasMaxLength(500);
        builder.Property(e => e.IsActive).HasDefaultValue(true);

        builder.HasIndex(e => e.Domain).IsUnique().HasDatabaseName("UX_EmailWhitelists_Domain");
    }
}
