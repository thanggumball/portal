using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentPortal.Repository.Entities;

namespace StudentPortal.Repository.Data.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.Property(t => t.Id).HasDefaultValueSql("NEWSEQUENTIALID()").ValueGeneratedOnAdd();
        builder.Property(t => t.TokenHash).HasColumnType("varchar(1000)").IsRequired();
        builder.Property(t => t.CreatedByIp).HasColumnType("varchar(45)");

        builder.HasOne(t => t.User).WithMany(u => u.RefreshTokens)
               .HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(t => t.TokenHash)
               .IsUnique()
               .IncludeProperties(t => new { t.UserId, t.ExpiresAt, t.RevokedAt })
               .HasDatabaseName("UX_RefreshTokens_TokenHash");

        builder.HasIndex(t => t.UserId)
               .HasFilter("[RevokedAt] IS NULL")
               .HasDatabaseName("IX_RefreshTokens_UserId_Active");
    }
}
