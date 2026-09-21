using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentPortal.Repository.Entities;

namespace StudentPortal.Repository.Data.Configurations;

public class AccountSequenceConfiguration : IEntityTypeConfiguration<AccountSequence>
{
    public void Configure(EntityTypeBuilder<AccountSequence> builder)
    {
        builder.Property(x => x.AccountType)
            .HasColumnType("varchar(20)")
            .IsRequired();

        builder.Property(x => x.NextNumber)
            .IsRequired();

        builder.HasIndex(x => x.AccountType)
            .IsUnique()
            .HasDatabaseName("UX_AccountSequences_AccountType");
    }
}
