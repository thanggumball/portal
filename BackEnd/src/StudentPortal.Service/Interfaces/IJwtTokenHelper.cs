namespace StudentPortal.Service.Interfaces;

public interface IJwtTokenHelper
{
    string GenerateAccessToken(
        string userId,
        string email,
        string role);
}