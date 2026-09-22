using MailService.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace MailService.API.Data;

public class MailDbContext : DbContext
{
    public MailDbContext(DbContextOptions<MailDbContext> options)
        : base(options)
    {
    }

    public DbSet<MailAccount> MailAccounts => Set<MailAccount>();
    public DbSet<Mail> Mails => Set<Mail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Mail>(entity =>
        {
            entity.ToTable("Mails");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.MessageId)
                .HasMaxLength(500)
                .IsRequired();

            entity.HasIndex(x => x.MessageId)
                .IsUnique();

            entity.Property(x => x.ThreadId)
                .HasMaxLength(500);

            entity.Property(x => x.InReplyTo)
                .HasMaxLength(500);

            entity.Property(x => x.References)
                .HasColumnType("nvarchar(max)");

            entity.Property(x => x.From)
                .HasMaxLength(1000)
                .IsRequired();

            entity.Property(x => x.To)
                .HasMaxLength(4000)
                .IsRequired();

            entity.Property(x => x.Cc)
                .HasMaxLength(4000);

            entity.Property(x => x.Bcc)
                .HasMaxLength(4000);

            entity.Property(x => x.ReplyTo)
                .HasMaxLength(1000);

            entity.Property(x => x.Subject)
                .HasMaxLength(1000);

            entity.Property(x => x.Body)
                .HasColumnType("nvarchar(max)");

            entity.Property(x => x.ErrorMessage)
                .HasColumnType("nvarchar(max)");

            entity.Property(x => x.Direction)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.CreatedAt)
                .IsRequired();
        });

        modelBuilder.Entity<MailAccount>(entity =>
        {
            entity.ToTable("MailAccounts");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Email)
                .HasMaxLength(320)
                .IsRequired();

            entity.HasIndex(x => x.Email)
                .IsUnique();

            entity.Property(x => x.PasswordHash)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(x => x.FullName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.IsActive)
                .IsRequired();

            entity.Property(x => x.CreatedAt)
                .IsRequired();

            entity.Property(x => x.UpdatedAt)
                .IsRequired();
        });
    }
}
