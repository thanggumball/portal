namespace StudentPortal.Repository.Entities;

public class EmailWhitelist : BaseEntity
{
    public string Email { get; set; } = string.Empty;

    public Guid RoleId { get; set; }

    public bool IsUsed { get; set; }

    public DateTime? UsedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public string? Note { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Role Role { get; set; } = null!;

    public User Creator { get; set; } = null!;
}