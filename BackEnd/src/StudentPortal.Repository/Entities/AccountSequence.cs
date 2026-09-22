namespace StudentPortal.Repository.Entities;

public class AccountSequence : BaseEntity
{
    public string AccountType { get; set; } = string.Empty;
    public long NextNumber { get; set; }
}
