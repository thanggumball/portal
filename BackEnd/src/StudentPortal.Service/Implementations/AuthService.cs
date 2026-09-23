using System.Security.Cryptography;
using StudentPortal.Common.DTOs.Auth;
using StudentPortal.Common.DTOs.Mail;
using StudentPortal.Common.DTOs.User;
using StudentPortal.Common.Enums;
using StudentPortal.Common.Exceptions;
using StudentPortal.Common.Settings;
using StudentPortal.Repository.Entities;
using StudentPortal.Repository.Interfaces;
using StudentPortal.Service.Helpers;
using StudentPortal.Service.Interfaces;


namespace StudentPortal.Service.Implementations;

public class AuthService : IAuthService
{
    private const string DefaultStudentRole = "Student";
    private readonly HttpClient _httpClient;

    private readonly IMailServiceClient _mailServiceClient;
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenHelper _jwtTokenHelper;
    private readonly JwtSettings _jwtSettings;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public AuthService(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IUnitOfWork unitOfWork,
    IRefreshTokenRepository refreshTokenRepository,
    IJwtTokenHelper jwtTokenHelper,
    JwtSettings jwtSettings,
    IMailServiceClient mailServiceClient)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtTokenHelper = jwtTokenHelper;
        _jwtSettings = jwtSettings;
        _mailServiceClient = mailServiceClient;
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

        var storedRefreshToken = await _refreshTokenRepository
            .FindByTokenHashAsync(tokenHash, ct);

        if (storedRefreshToken is null)
        {
            throw new UnauthorizedAccessException(
                "Invalid refresh token.");
        }

        if (storedRefreshToken.RevokedAt.HasValue ||
            storedRefreshToken.ExpiresAt <= DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException(
                "Refresh token is expired or revoked.");
        }

        var user = await _userRepository.GetByIdWithRoleAsync(
            storedRefreshToken.UserId,
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

        var newRefreshToken = Convert.ToBase64String(
            RandomNumberGenerator.GetBytes(64));

        var newRefreshTokenHash = Convert.ToHexString(
            SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(newRefreshToken)));

        storedRefreshToken.RevokedAt = DateTime.UtcNow;
        _refreshTokenRepository.Update(storedRefreshToken);

        var newRefreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = newRefreshTokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(
                _jwtSettings.RefreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow
        };

        await _refreshTokenRepository.AddAsync(
            newRefreshTokenEntity,
            ct);

        await _unitOfWork.SaveChangesAsync(ct);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(
                _jwtSettings.AccessTokenExpirationMinutes)
        };
    }

    public async Task LogoutAsync(
     LogoutRequest request,
     CancellationToken ct = default)
    {
        var tokenHash = Convert.ToHexString(
            SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(request.RefreshToken)));

        var refreshToken = await _refreshTokenRepository
            .FindByTokenHashAsync(tokenHash, ct);

        if (refreshToken is null)
        {
            return;
        }

        if (!refreshToken.RevokedAt.HasValue)
        {
            refreshToken.RevokedAt = DateTime.UtcNow;

            _refreshTokenRepository.Update(refreshToken);

            await _unitOfWork.SaveChangesAsync(ct);
        }
    }

    public async Task ChangePasswordAsync(
        Guid userId,
        ChangePasswordRequest request,
        CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct);

        if (user is null || user.IsDeleted)
        {
            throw new NotFoundException("User not found.");
        }

        if (!BCrypt.Net.BCrypt.Verify(
                request.CurrentPassword,
                user.PasswordHash))
        {
            throw new UnauthorizedAccessException(
                "Current password is incorrect.");
        }

        if (BCrypt.Net.BCrypt.Verify(
                request.NewPassword,
                user.PasswordHash))
        {
            throw new BadRequestException(
                "New password must be different from current password.");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(
            request.NewPassword);

        user.UpdatedAt = DateTime.UtcNow;

        _userRepository.Update(user);

        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task ForgotPasswordAsync(
    ForgotPasswordRequest request,
    CancellationToken ct = default)
    {
        var user = await _userRepository.GetForLoginAsync(
            request.Email,
            ct);

        if (user is null ||
            user.IsDeleted ||
            user.Status != UserStatus.Active)
        {
            throw new NotFoundException(
                "User with this email was not found.");
        }

        var temporaryPassword =
            Convert.ToBase64String(
                RandomNumberGenerator.GetBytes(9));

        await using var transaction =
            await _unitOfWork.BeginTransactionAsync(ct);

        try
        {
            await _mailServiceClient.SendMailAsync(
                new SendMailRequest
                {
                    To = user.Email,
                    Subject = "Student Portal - Password Reset",
                    Body =
                        $"Hello {user.FullName},\n\n" +
                        "Your Student Portal temporary password is:\n\n" +
                        $"{temporaryPassword}\n\n" +
                        "Please use this password to log in to Student Portal.",
                    IsHtml = false
                },
                ct);

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(
                temporaryPassword);

            user.UpdatedAt = DateTime.UtcNow;

            _userRepository.Update(user);

            await _unitOfWork.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }
}
