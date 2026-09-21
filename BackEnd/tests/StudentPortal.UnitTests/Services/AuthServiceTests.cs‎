using FluentAssertions;
using Moq;
using StudentPortal.Common.DTOs.Auth;
using StudentPortal.Common.Enums;
using StudentPortal.Common.Exceptions;
using StudentPortal.Common.Settings;
using StudentPortal.Repository.Entities;
using StudentPortal.Repository.Interfaces;
using StudentPortal.Service.Implementations;
using StudentPortal.Service.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace StudentPortal.UnitTests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IRoleRepository> _roleRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepository = new();
    private readonly Mock<IJwtTokenHelper> _jwtTokenHelper = new();

    private readonly JwtSettings _jwtSettings = new()
    {
        SecretKey = "test-secret-key",
        Issuer = "StudentPortal.API",
        Audience = "StudentPortal.Client",
        AccessTokenExpirationMinutes = 15,
        RefreshTokenExpirationDays = 7
    };

    private AuthService CreateService()
    {
        return new AuthService(
            _userRepository.Object,
            _roleRepository.Object,
            _unitOfWork.Object,
            _refreshTokenRepository.Object,
            _jwtTokenHelper.Object,
            _jwtSettings);
    }

    private static Role CreateStudentRole()
    {
        return new Role
        {
            Id = Guid.NewGuid(),
            Name = "Student"
        };
    }

    private static User CreateActiveUser(Role? role = null)
    {
        role ??= CreateStudentRole();

        return new User
        {
            Id = Guid.NewGuid(),
            Email = "student@example.com",
            UserName = "student01",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            FullName = "Test Student",
            StudentCode = "SE123456",
            RoleId = role.Id,
            Role = role,
            Status = UserStatus.Active,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    // =========================================================
    // REGISTER
    // =========================================================

    [Fact]
    public async Task RegisterAsync_WithValidRequest_ShouldCreateStudentAndReturnUser()
    {
        // Arrange
        var role = CreateStudentRole();

        var request = new RegisterRequest
        {
            Email = "student@example.com",
            UserName = "student01",
            Password = "Password123!",
            FullName = "Test Student",
            StudentCode = "SE123456"
        };

        _userRepository
            .Setup(x => x.ExistsByEmailAsync(
                request.Email,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _userRepository
            .Setup(x => x.FindByUserNameAsync(
                request.UserName,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _roleRepository
            .Setup(x => x.FindByNameAsync(
                "Student",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var service = CreateService();

        // Act
        var result = await service.RegisterAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(request.Email);
        result.UserName.Should().Be(request.UserName);
        result.FullName.Should().Be(request.FullName);
        result.StudentCode.Should().Be(request.StudentCode);
        result.RoleName.Should().Be("Student");
        result.Status.Should().Be(UserStatus.Active);

        _userRepository.Verify(
            x => x.AddAsync(
                It.IsAny<User>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);

        var addedUser = _userRepository.Invocations
            .Single(x => x.Method.Name == nameof(IUserRepository.AddAsync))
            .Arguments[0]
            .Should()
            .BeOfType<User>()
            .Subject;

        addedUser.Email.Should().Be(request.Email);
        addedUser.UserName.Should().Be(request.UserName);
        addedUser.RoleId.Should().Be(role.Id);
        addedUser.Status.Should().Be(UserStatus.Active);
        addedUser.IsDeleted.Should().BeFalse();

        BCrypt.Net.BCrypt.Verify(
            request.Password,
            addedUser.PasswordHash)
            .Should().BeTrue();
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "existing@example.com",
            UserName = "student01",
            Password = "Password123!",
            FullName = "Test Student"
        };

        _userRepository
            .Setup(x => x.ExistsByEmailAsync(
                request.Email,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = CreateService();

        // Act
        var act = () => service.RegisterAsync(request);

        // Assert
        await act.Should()
            .ThrowAsync<ConflictException>()
            .WithMessage("An account with this email already exists.");

        _userRepository.Verify(
            x => x.AddAsync(
                It.IsAny<User>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WhenUsernameAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "student@example.com",
            UserName = "existingUser",
            Password = "Password123!",
            FullName = "Test Student"
        };

        _userRepository
            .Setup(x => x.ExistsByEmailAsync(
                request.Email,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _userRepository
            .Setup(x => x.FindByUserNameAsync(
                request.UserName,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User());

        var service = CreateService();

        // Act
        var act = () => service.RegisterAsync(request);

        // Assert
        await act.Should()
            .ThrowAsync<ConflictException>()
            .WithMessage("An account with this username already exists.");

        _userRepository.Verify(
            x => x.AddAsync(
                It.IsAny<User>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WhenStudentRoleDoesNotExist_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "student@example.com",
            UserName = "student01",
            Password = "Password123!",
            FullName = "Test Student"
        };

        _userRepository
            .Setup(x => x.ExistsByEmailAsync(
                request.Email,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _userRepository
            .Setup(x => x.FindByUserNameAsync(
                request.UserName,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _roleRepository
            .Setup(x => x.FindByNameAsync(
                "Student",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);

        var service = CreateService();

        // Act
        var act = () => service.RegisterAsync(request);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Default Student role was not found.");

        _userRepository.Verify(
            x => x.AddAsync(
                It.IsAny<User>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // =========================================================
    // LOGIN
    // =========================================================

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnTokenPair()
    {
        // Arrange
        var user = CreateActiveUser();

        var request = new LoginRequest
        {
            Email = user.Email,
            Password = "Password123!"
        };

        _userRepository
            .Setup(x => x.GetForLoginAsync(
                request.Email,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _jwtTokenHelper
            .Setup(x => x.GenerateAccessToken(
                user.Id.ToString(),
                user.Email,
                user.Role.Name))
            .Returns("access-token");

        _unitOfWork
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var service = CreateService();

        // Act
        var result = await service.LoginAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);

        _refreshTokenRepository.Verify(
            x => x.AddAsync(
                It.IsAny<RefreshToken>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _userRepository.Verify(
            x => x.Update(user),
            Times.Once);

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);

        user.LastLoginAt.Should().NotBeNull();
    }

    [Fact]
    public async Task LoginAsync_WhenUserDoesNotExist_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "unknown@example.com",
            Password = "Password123!"
        };

        _userRepository
            .Setup(x => x.GetForLoginAsync(
                request.Email,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var service = CreateService();

        // Act
        var act = () => service.LoginAsync(request);

        // Assert
        await act.Should()
            .ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid email or password.");

        _jwtTokenHelper.Verify(
            x => x.GenerateAccessToken(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordIsIncorrect_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var user = CreateActiveUser();

        var request = new LoginRequest
        {
            Email = user.Email,
            Password = "WrongPassword!"
        };

        _userRepository
            .Setup(x => x.GetForLoginAsync(
                request.Email,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var service = CreateService();

        // Act
        var act = () => service.LoginAsync(request);

        // Assert
        await act.Should()
            .ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid email or password.");
    }

    [Fact]
    public async Task LoginAsync_WhenUserIsInactive_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var user = CreateActiveUser();
        user.Status = UserStatus.Inactive;

        var request = new LoginRequest
        {
            Email = user.Email,
            Password = "Password123!"
        };

        _userRepository
            .Setup(x => x.GetForLoginAsync(
                request.Email,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var service = CreateService();

        // Act
        var act = () => service.LoginAsync(request);

        // Assert
        await act.Should()
            .ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Your account is not active.");

        _jwtTokenHelper.Verify(
            x => x.GenerateAccessToken(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Never);

        _refreshTokenRepository.Verify(
            x => x.AddAsync(
                It.IsAny<RefreshToken>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // =========================================================
    // REFRESH TOKEN
    // =========================================================

    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_ShouldRotateRefreshToken()
    {
        // Arrange
        var user = CreateActiveUser();

        var rawToken = "old-refresh-token";

        var storedToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = HashToken(rawToken),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        var request = new RefreshTokenRequest
        {
            RefreshToken = rawToken
        };

        _refreshTokenRepository
            .Setup(x => x.FindByTokenHashAsync(
                HashToken(rawToken),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        _userRepository
            .Setup(x => x.GetByIdWithRoleAsync(
                user.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _jwtTokenHelper
            .Setup(x => x.GenerateAccessToken(
                user.Id.ToString(),
                user.Email,
                user.Role.Name))
            .Returns("new-access-token");

        _unitOfWork
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var service = CreateService();

        // Act
        var result = await service.RefreshTokenAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("new-access-token");
        result.RefreshToken.Should().NotBe(rawToken);
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();

        storedToken.RevokedAt.Should().NotBeNull();

        _refreshTokenRepository.Verify(
            x => x.Update(storedToken),
            Times.Once);

        _refreshTokenRepository.Verify(
            x => x.AddAsync(
                It.IsAny<RefreshToken>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);

        var addedToken = _refreshTokenRepository.Invocations
            .Where(x => x.Method.Name == nameof(IRefreshTokenRepository.AddAsync))
            .Select(x => x.Arguments[0])
            .OfType<RefreshToken>()
            .Single();

        addedToken.UserId.Should().Be(user.Id);
        addedToken.TokenHash.Should().Be(HashToken(result.RefreshToken));
        addedToken.TokenHash.Should().NotBe(storedToken.TokenHash);
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenTokenDoesNotExist_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var rawToken = "invalid-refresh-token";

        _refreshTokenRepository
            .Setup(x => x.FindByTokenHashAsync(
                HashToken(rawToken),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        var service = CreateService();

        // Act
        var act = () => service.RefreshTokenAsync(
            new RefreshTokenRequest
            {
                RefreshToken = rawToken
            });

        // Assert
        await act.Should()
            .ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid refresh token.");

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenTokenIsRevoked_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var rawToken = "revoked-refresh-token";

        var storedToken = new RefreshToken
        {
            UserId = Guid.NewGuid(),
            TokenHash = HashToken(rawToken),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            RevokedAt = DateTime.UtcNow.AddMinutes(-1)
        };

        _refreshTokenRepository
            .Setup(x => x.FindByTokenHashAsync(
                HashToken(rawToken),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        var service = CreateService();

        // Act
        var act = () => service.RefreshTokenAsync(
            new RefreshTokenRequest
            {
                RefreshToken = rawToken
            });

        // Assert
        await act.Should()
            .ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Refresh token is expired or revoked.");

        _userRepository.Verify(
            x => x.GetByIdWithRoleAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenTokenIsExpired_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var rawToken = "expired-refresh-token";

        var storedToken = new RefreshToken
        {
            UserId = Guid.NewGuid(),
            TokenHash = HashToken(rawToken),
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1)
        };

        _refreshTokenRepository
            .Setup(x => x.FindByTokenHashAsync(
                HashToken(rawToken),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        var service = CreateService();

        // Act
        var act = () => service.RefreshTokenAsync(
            new RefreshTokenRequest
            {
                RefreshToken = rawToken
            });

        // Assert
        await act.Should()
            .ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Refresh token is expired or revoked.");

        _userRepository.Verify(
            x => x.GetByIdWithRoleAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenUserIsNotFound_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var rawToken = "valid-refresh-token";

        var storedToken = new RefreshToken
        {
            UserId = Guid.NewGuid(),
            TokenHash = HashToken(rawToken),
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _refreshTokenRepository
            .Setup(x => x.FindByTokenHashAsync(
                HashToken(rawToken),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        _userRepository
            .Setup(x => x.GetByIdWithRoleAsync(
                storedToken.UserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var service = CreateService();

        // Act
        var act = () => service.RefreshTokenAsync(
            new RefreshTokenRequest
            {
                RefreshToken = rawToken
            });

        // Assert
        await act.Should()
            .ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("User account is not active.");

        _refreshTokenRepository.Verify(
            x => x.AddAsync(
                It.IsAny<RefreshToken>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenUserIsInactive_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var rawToken = "valid-refresh-token";

        var user = CreateActiveUser();
        user.Status = UserStatus.Inactive;

        var storedToken = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = HashToken(rawToken),
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _refreshTokenRepository
            .Setup(x => x.FindByTokenHashAsync(
                HashToken(rawToken),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        _userRepository
            .Setup(x => x.GetByIdWithRoleAsync(
                user.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var service = CreateService();

        // Act
        var act = () => service.RefreshTokenAsync(
            new RefreshTokenRequest
            {
                RefreshToken = rawToken
            });

        // Assert
        await act.Should()
            .ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("User account is not active.");

        _refreshTokenRepository.Verify(
            x => x.AddAsync(
                It.IsAny<RefreshToken>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // =========================================================
    // LOGOUT
    // =========================================================

    [Fact]
    public async Task LogoutAsync_WithValidToken_ShouldRevokeToken()
    {
        // Arrange
        var rawToken = "refresh-token";

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            TokenHash = HashToken(rawToken),
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _refreshTokenRepository
            .Setup(x => x.FindByTokenHashAsync(
                HashToken(rawToken),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var service = CreateService();

        // Act
        await service.LogoutAsync(
            new LogoutRequest
            {
                RefreshToken = rawToken
            });

        // Assert
        refreshToken.RevokedAt.Should().NotBeNull();

        _refreshTokenRepository.Verify(
            x => x.Update(refreshToken),
            Times.Once);

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task LogoutAsync_WhenTokenDoesNotExist_ShouldDoNothing()
    {
        // Arrange
        var rawToken = "unknown-token";

        _refreshTokenRepository
            .Setup(x => x.FindByTokenHashAsync(
                HashToken(rawToken),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        var service = CreateService();

        // Act
        await service.LogoutAsync(
            new LogoutRequest
            {
                RefreshToken = rawToken
            });

        // Assert
        _refreshTokenRepository.Verify(
            x => x.Update(It.IsAny<RefreshToken>()),
            Times.Never);

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task LogoutAsync_WhenTokenIsAlreadyRevoked_ShouldDoNothing()
    {
        // Arrange
        var rawToken = "already-revoked-token";

        var revokedAt = DateTime.UtcNow.AddMinutes(-10);

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            TokenHash = HashToken(rawToken),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            RevokedAt = revokedAt
        };

        _refreshTokenRepository
            .Setup(x => x.FindByTokenHashAsync(
                HashToken(rawToken),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);

        var service = CreateService();

        // Act
        await service.LogoutAsync(
            new LogoutRequest
            {
                RefreshToken = rawToken
            });

        // Assert
        refreshToken.RevokedAt.Should().Be(revokedAt);

        _refreshTokenRepository.Verify(
            x => x.Update(It.IsAny<RefreshToken>()),
            Times.Never);

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // =========================================================
    // CHANGE PASSWORD
    // =========================================================

    [Fact]
    public async Task ChangePasswordAsync_WithValidRequest_ShouldUpdatePassword()
    {
        // Arrange
        var user = CreateActiveUser();

        var oldHash = user.PasswordHash;

        var request = new ChangePasswordRequest
        {
            CurrentPassword = "Password123!",
            NewPassword = "NewPassword456!"
        };

        _userRepository
            .Setup(x => x.GetByIdAsync(
                user.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var service = CreateService();

        // Act
        await service.ChangePasswordAsync(
            user.Id,
            request);

        // Assert
        user.PasswordHash.Should().NotBe(oldHash);

        BCrypt.Net.BCrypt.Verify(
            request.NewPassword,
            user.PasswordHash)
            .Should().BeTrue();

        user.UpdatedAt.Should().BeAfter(
            DateTime.UtcNow.AddMinutes(-1));

        _userRepository.Verify(
            x => x.Update(user),
            Times.Once);

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenUserDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _userRepository
            .Setup(x => x.GetByIdAsync(
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var request = new ChangePasswordRequest
        {
            CurrentPassword = "Password123!",
            NewPassword = "NewPassword456!"
        };

        var service = CreateService();

        // Act
        var act = () => service.ChangePasswordAsync(
            userId,
            request);

        // Assert
        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage("User not found.");

        _userRepository.Verify(
            x => x.Update(It.IsAny<User>()),
            Times.Never);
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenUserIsDeleted_ShouldThrowNotFoundException()
    {
        // Arrange
        var user = CreateActiveUser();
        user.IsDeleted = true;

        _userRepository
            .Setup(x => x.GetByIdAsync(
                user.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var request = new ChangePasswordRequest
        {
            CurrentPassword = "Password123!",
            NewPassword = "NewPassword456!"
        };

        var service = CreateService();

        // Act
        var act = () => service.ChangePasswordAsync(
            user.Id,
            request);

        // Assert
        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage("User not found.");

        _userRepository.Verify(
            x => x.Update(It.IsAny<User>()),
            Times.Never);
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenCurrentPasswordIsIncorrect_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var user = CreateActiveUser();

        _userRepository
            .Setup(x => x.GetByIdAsync(
                user.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var request = new ChangePasswordRequest
        {
            CurrentPassword = "WrongPassword!",
            NewPassword = "NewPassword456!"
        };

        var service = CreateService();

        // Act
        var act = () => service.ChangePasswordAsync(
            user.Id,
            request);

        // Assert
        await act.Should()
            .ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Current password is incorrect.");

        _userRepository.Verify(
            x => x.Update(It.IsAny<User>()),
            Times.Never);

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenNewPasswordIsSameAsCurrent_ShouldThrowBadRequestException()
    {
        // Arrange
        var user = CreateActiveUser();

        _userRepository
            .Setup(x => x.GetByIdAsync(
                user.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var request = new ChangePasswordRequest
        {
            CurrentPassword = "Password123!",
            NewPassword = "Password123!"
        };

        var service = CreateService();

        // Act
        var act = () => service.ChangePasswordAsync(
            user.Id,
            request);

        // Assert
        await act.Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage(
                "New password must be different from current password.");

        _userRepository.Verify(
            x => x.Update(It.IsAny<User>()),
            Times.Never);

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // =========================================================
    // HELPERS
    // =========================================================

    private static string HashToken(string token)
    {
        return Convert.ToHexString(
            SHA256.HashData(
                Encoding.UTF8.GetBytes(token)));
    }
}