using StudentPortal.Common.DTOs.Auth;
using StudentPortal.Common.DTOs.User;
using StudentPortal.Common.Enums;
using StudentPortal.Repository.Entities;
using StudentPortal.Repository.Interfaces;
using StudentPortal.Service.Interfaces;
using System.Security.Cryptography;
using StudentPortal.Service.Helpers;
using StudentPortal.Common.Settings;

namespace StudentPortal.Service.Implementations;

public class AuthService : IAuthService
{
    private const string DefaultStudentRole = "Student";

    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;

    private readonly JwtTokenHelper _jwtTokenHelper;
    private readonly JwtSettings _jwtSettings;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public AuthService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IUnitOfWork unitOfWork,
        IRefreshTokenRepository refreshTokenRepository,
        JwtTokenHelper jwtTokenHelper,
        JwtSettings jwtSettings)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtTokenHelper = jwtTokenHelper;
        _jwtSettings = jwtSettings;
    }

    public async Task<UserResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken ct = default)
    {
        var emailExists = await _userRepository.ExistsByEmailAsync(
            request.Email,
            ct);

        if (emailExists)
        {
            throw new InvalidOperationException(
                "An account with this email already exists.");
        }

        var userNameExists = await _userRepository.FindByUserNameAsync(
            request.UserName,
            ct);

        if (userNameExists is not null)
        {
            throw new InvalidOperationException(
                "An account with this username already exists.");
        }

        var role = await _roleRepository.FindByNameAsync(
            DefaultStudentRole,
            ct);

        if (role is null)
        {
            throw new InvalidOperationException(
                "Default Student role was not found.");
        }

        var user = new User
        {
            Email = request.Email,
            UserName = request.UserName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = request.FullName,
            StudentCode = request.StudentCode,
            RoleId = role.Id,
            Role = role,
            Status = UserStatus.Active,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            UserName = user.UserName,
            FullName = user.FullName,
            StudentCode = user.StudentCode,
            RoleName = role.Name,
            Status = user.Status,
            AvatarUrl = user.AvatarUrl,
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken ct = default)
    {
        var user = await _userRepository.GetForLoginAsync(
            request.Email,
            ct);

        if (user is null ||
            !BCrypt.Net.BCrypt.Verify(
                request.Password,
                user.PasswordHash))
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        if (user.Status != UserStatus.Active)
        {
            throw new UnauthorizedAccessException(
                "Your account is not active.");
        }

        var accessToken = _jwtTokenHelper.GenerateAccessToken(
            user.Id.ToString(),
            user.Email,
            user.Role.Name);

        var refreshToken = Convert.ToBase64String(
            RandomNumberGenerator.GetBytes(64));

        var refreshTokenHash = Convert.ToHexString(
            SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(refreshToken)));

        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshTokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(
                _jwtSettings.RefreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow
        };

        await _refreshTokenRepository.AddAsync(
            refreshTokenEntity,
            ct);

        user.LastLoginAt = DateTime.UtcNow;
        _userRepository.Update(user);

        await _unitOfWork.SaveChangesAsync(ct);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(
                _jwtSettings.AccessTokenExpirationMinutes)
        };
    }

    public async Task<LoginResponse> RefreshTokenAsync(
    RefreshTokenRequest request,
    CancellationToken ct = default)
    {
        var tokenHash = Convert.ToHexString(
            SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(request.RefreshToken)));

        var refreshToken = await _refreshTokenRepository
            .FindByTokenHashAsync(tokenHash, ct);

        if (refreshToken is null)
        {
            throw new UnauthorizedAccessException(
                "Invalid refresh token.");
        }

        if (refreshToken.RevokedAt.HasValue ||
            refreshToken.ExpiresAt <= DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException(
                "Refresh token is expired or revoked.");
        }

        var user = await _userRepository.GetByIdWithRoleAsync(
            refreshToken.UserId,
            ct);

        if (user is null ||
            user.IsDeleted ||
            user.Status != UserStatus.Active)
        {
            throw new UnauthorizedAccessException(
                "User account is not active.");
        }

        var accessToken = _jwtTokenHelper.GenerateAccessToken(
            user.Id.ToString(),
            user.Email,
            user.Role.Name);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = request.RefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(
                _jwtSettings.AccessTokenExpirationMinutes)
        };
    }

    public Task LogoutAsync(
        LogoutRequest request,
        CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task ChangePasswordAsync(
        Guid userId,
        ChangePasswordRequest request,
        CancellationToken ct = default)
        => throw new NotImplementedException();
}