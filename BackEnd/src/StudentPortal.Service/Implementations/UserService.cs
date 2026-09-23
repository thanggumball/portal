using StudentPortal.Common.DTOs.Mail;
using StudentPortal.Common.DTOs.Shared;
using StudentPortal.Common.DTOs.User;
using StudentPortal.Common.Enums;
using StudentPortal.Common.Exceptions;
using StudentPortal.Repository.Entities;
using StudentPortal.Repository.Interfaces;
using StudentPortal.Service.Interfaces;
using StudentPortal.Service.Mappers;

namespace StudentPortal.Service.Implementations;

public class UserService : IUserService
{
    private const string StaffRole = "Staff";
    private const string StudentRole = "Student";
    private const string DefaultPassword = "Avepoint2026@";

    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IAccountSequenceRepository _accountSequenceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMailServiceClient _mailServiceClient;

    public UserService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IAccountSequenceRepository accountSequenceRepository,
        IUnitOfWork unitOfWork,
        IMailServiceClient mailServiceClient)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _accountSequenceRepository = accountSequenceRepository;
        _unitOfWork = unitOfWork;
        _mailServiceClient = mailServiceClient;
    }

    public async Task<UserResponse> CreateUserAsync(
    CreateUserRequest request,
    CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            throw new BadRequestException("Full name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.RoleName))
        {
            throw new BadRequestException("Role is required.");
        }

        var roleName = request.RoleName.Trim();

        if (roleName != StaffRole && roleName != StudentRole)
        {
            throw new BadRequestException(
                "Role must be either Staff or Student.");
        }

        var role = await _roleRepository.FindByNameAsync(
            roleName,
            ct);

        if (role is null)
        {
            throw new InvalidOperationException(
                $"Role '{roleName}' was not found.");
        }

        const int maxAttempts = 100000;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            await using var transaction =
                await _unitOfWork.BeginTransactionAsync(ct);

            try
            {
                var sequence = await _accountSequenceRepository
                    .GetForUpdateAsync(roleName, ct);

                if (sequence is null)
                {
                    throw new InvalidOperationException(
                        $"Account sequence for role '{roleName}' was not found.");
                }

                var userCode = sequence.NextNumber.ToString();

                var domain = roleName == StaffRole
                    ? "staff.avepoint.com"
                    : "student.avepoint.com";

                var email = $"{userCode}@{domain}";

                if (await _userRepository.ExistsByEmailAsync(email, ct))
                {
                    sequence.NextNumber++;

                    _accountSequenceRepository.Update(sequence);

                    await _unitOfWork.SaveChangesAsync(ct);
                    await transaction.CommitAsync(ct);

                    continue;
                }

                sequence.NextNumber++;

                var now = DateTime.UtcNow;

                var user = new User
                {
                    Email = email,
                    UserName = userCode,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(
                        DefaultPassword),
                    FullName = request.FullName.Trim(),
                    UserCode = userCode,
                    RoleId = role.Id,
                    Role = role,
                    Status = UserStatus.Active,
                    IsDeleted = false,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                _accountSequenceRepository.Update(sequence);
                await _userRepository.AddAsync(user, ct);

                await _unitOfWork.SaveChangesAsync(ct);

                try
                {
                    await _mailServiceClient.CreateAccountAsync(
                        new CreateMailAccountRequest
                        {
                            Email = email,
                            Password = DefaultPassword,
                            FullName = user.FullName
                        },
                        ct);
                }
                catch (InvalidOperationException ex)
                    when (ex.Message == "Mail account already exists.")
                {
                    // The MailService account already exists,
                    // so keep the consumed sequence number
                    // and retry with the next number.
                    await transaction.CommitAsync(ct);

                    if (attempt == maxAttempts)
                    {
                        throw new ConflictException(
                            "Unable to generate a unique account number " +
                            "after multiple attempts.");
                    }

                    continue;
                }

                await transaction.CommitAsync(ct);

                return new UserResponse
                {
                    Id = user.Id,
                    Email = user.Email,
                    UserName = user.UserName,
                    FullName = user.FullName,
                    UserCode = user.UserCode,
                    RoleName = role.Name,
                    Status = user.Status,
                    AvatarUrl = user.AvatarUrl,
                    LastLoginAt = user.LastLoginAt,
                    CreatedAt = user.CreatedAt
                };
            }
            catch (ConflictException)
            {
                throw;
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }

        throw new ConflictException(
            "Unable to generate a unique account number.");
    }

    public async Task<UserResponse> UpdateUserAsync(
        Guid userId,
        UpdateUserRequest request,
        CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct);

        if (user is null || user.IsDeleted)
        {
            throw new NotFoundException("User not found.");
        }

        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            throw new BadRequestException("Full name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.RoleName))
        {
            throw new BadRequestException("Role is required.");
        }

        var roleName = request.RoleName.Trim();

        if (roleName != StaffRole && roleName != StudentRole)
        {
            throw new BadRequestException(
                "Role must be either Staff or Student.");
        }

        var role = await _roleRepository.FindByNameAsync(
            roleName,
            ct);

        if (role is null)
        {
            throw new NotFoundException(
                $"Role '{roleName}' was not found.");
        }

        user.FullName = request.FullName.Trim();
        user.RoleId = role.Id;
        user.Role = role;
        user.Status = request.Status;
        user.AvatarUrl = request.AvatarUrl;
        user.UpdatedAt = DateTime.UtcNow;

        _userRepository.Update(user);

        await _unitOfWork.SaveChangesAsync(ct);

        return UserMapper.ToResponse(user);
    }

    public async Task<PagedResult<UserResponse>> SearchUsersAsync(
    UserFilter filter,
    CancellationToken ct = default)
    {
        var (users, total) = await _userRepository.SearchAsync(filter, ct);

        return new PagedResult<UserResponse>
        {
            Items = users.Select(UserMapper.ToResponse).ToList(),
            Total = total,
            Page = filter.Page,
            PageSize = filter.PageSize,
        };
    }
}
