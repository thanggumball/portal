using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentPortal.Repository.Entities;

namespace StudentPortal.Repository.Data.Configurations;

public class UserMfaConfiguration : IEntityTypeConfiguration<UserMfa>
{
    public void Configure(EntityTypeBuilder<UserMfa> builder)
    {
        builder.Property(m => m.Id).HasDefaultValueSql("NEWSEQUENTIALID()").ValueGeneratedOnAdd();
        builder.Property(m => m.SecretKey).HasColumnType("varchar(500)").IsRequired();
        builder.Property(m => m.IsEnabled).HasDefaultValue(false);

        builder.HasOne(m => m.User).WithOne()
               .HasForeignKey<UserMfa>(m => m.UserId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => m.UserId).IsUnique().HasDatabaseName("UX_UserMfa_UserId");
    }
}
