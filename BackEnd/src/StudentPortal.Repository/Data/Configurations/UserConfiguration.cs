using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentPortal.Repository.Entities;

namespace StudentPortal.Repository.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.Id).HasDefaultValueSql("NEWSEQUENTIALID()").ValueGeneratedOnAdd();

        builder.Property(u => u.Email).HasColumnType("varchar(255)").IsRequired();
        builder.Property(u => u.UserName).HasColumnType("varchar(100)").IsRequired();
        builder.Property(u => u.PasswordHash).HasColumnType("varchar(500)").IsRequired();
        builder.Property(u => u.StudentCode).HasColumnType("varchar(50)");
        builder.Property(u => u.AvatarUrl).HasColumnType("varchar(1000)");
        builder.Property(u => u.FullName).HasMaxLength(255).IsRequired();
        builder.Property(u => u.IsDeleted).HasDefaultValue(false);

        builder.HasOne(u => u.Role).WithMany(r => r.Users)
               .HasForeignKey(u => u.RoleId).OnDelete(DeleteBehavior.Restrict);

        // Login, duplicate email check. Filtered so a soft-deleted user's email can be reused.
        builder.HasIndex(u => u.Email)
               .IsUnique().HasFilter("[IsDeleted] = 0")
               .HasDatabaseName("UX_Users_Email");

        builder.HasIndex(u => u.UserName)
               .IsUnique().HasFilter("[IsDeleted] = 0")
               .HasDatabaseName("UX_Users_UserName");

        // User Management screen: filter by RoleId, sort CreatedAt descending, paginate
        builder.HasIndex(u => new { u.RoleId, u.CreatedAt })
               .IsDescending(false, true)
               .IncludeProperties(u => new { u.Email, u.FullName, u.StudentCode, u.LastLoginAt })
               .HasFilter("[IsDeleted] = 0")
               .HasDatabaseName("IX_Users_RoleId_CreatedAt");

        builder.HasIndex(u => u.StudentCode)
               .IsUnique()
               .HasFilter("[IsDeleted] = 0 AND [StudentCode] IS NOT NULL")
               .HasDatabaseName("UX_Users_StudentCode");
    }
}
