namespace StudentPortal.Common.DTOs.EmailWhitelist;

public class CreateEmailWhitelistRequest
{
    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public string? Note { get; set; }
}