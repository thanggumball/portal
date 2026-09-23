namespace StudentPortal.Common.Constants;

/// <summary>
/// Custom field names carried on the audit event. They live in Common because the API
/// layer writes them and the Repository layer reads them - both sides must agree.
/// </summary>
public static class AuditFields
{
    public const string UserId = "UserId";
    public const string IpAddress = "IpAddress";
}
