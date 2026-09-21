namespace StudentPortal.Repository.Entities;

public class EmailWhitelist : BaseEntity
{
    public string Domain { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? Note { get; set; }
    public DateTime UpdatedAt { get; set; }
}
