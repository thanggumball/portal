using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentPortal.Repository.Entities;

namespace StudentPortal.Repository.Data.Configurations;

public class EmailWhitelistConfiguration : IEntityTypeConfiguration<EmailWhitelist>
{
    public void Configure(EntityTypeBuilder<EmailWhitelist> builder)
    {
        builder.Property(e => e.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.Email)
            .HasColumnType("varchar(255)")
            .IsRequired();

        builder.Property(e => e.RoleId)
            .IsRequired();

        builder.Property(e => e.IsUsed)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(e => e.UsedAt)
            .IsRequired(false);

        builder.Property(e => e.CreatedBy)
            .IsRequired();

        builder.Property(e => e.Note)
            .HasMaxLength(500);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .IsRequired();

        builder.HasIndex(e => e.Email)
            .IsUnique()
            .HasDatabaseName("UX_EmailWhitelists_Email");

        builder.HasOne(e => e.Role)
            .WithMany()
            .HasForeignKey(e => e.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Creator)
            .WithMany()
            .HasForeignKey(e => e.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}