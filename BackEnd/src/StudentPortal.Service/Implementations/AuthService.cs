using StudentPortal.Common.DTOs.Auth;
using StudentPortal.Common.DTOs.User;
using StudentPortal.Service.Interfaces;

namespace StudentPortal.Service.Implementations;

public class AuthService : IAuthService
{
    public Task<UserResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task LogoutAsync(LogoutRequest request, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default)
        => throw new NotImplementedException();
}
