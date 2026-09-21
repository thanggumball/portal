using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentPortal.Repository.Entities;

namespace StudentPortal.Repository.Data.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.Property(r => r.Id).HasDefaultValueSql("NEWSEQUENTIALID()").ValueGeneratedOnAdd();
        builder.Property(r => r.Name).HasColumnType("varchar(50)").IsRequired();
        builder.Property(r => r.Description).HasMaxLength(255);

        builder.HasIndex(r => r.Name).IsUnique().HasDatabaseName("UX_Roles_Name");
    }
}
