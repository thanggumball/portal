using StudentPortal.Common.Constants;
using StudentPortal.Common.DTOs.EmailWhitelist;
using StudentPortal.Repository.Entities;
using StudentPortal.Repository.Interfaces;
using StudentPortal.Service.Interfaces;
using StudentPortal.Common.Exceptions;

namespace StudentPortal.Service.Implementations;

public class EmailWhitelistService : IEmailWhitelistService
{
    private const string StaffRole = RoleConstants.Admin;
    private const string StudentRole = RoleConstants.Student;

    private readonly IEmailWhitelistRepository _emailWhitelistRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EmailWhitelistService(
        IEmailWhitelistRepository emailWhitelistRepository,
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IUnitOfWork unitOfWork)
    {
        _emailWhitelistRepository = emailWhitelistRepository;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task CreateAsync(
        CreateEmailWhitelistRequest request,
        Guid createdBy,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new BadRequestException("Email is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Role))
        {
            throw new BadRequestException("Role is required.");
        }

        var email = request.Email.Trim().ToLowerInvariant();
        var roleName = request.Role.Trim();

        if (roleName != StaffRole && roleName != StudentRole)
        {
            throw new BadRequestException(
                "Role must be either Staff or Student.");
        }

        if (await _userRepository.ExistsByEmailAsync(email, ct))
        {
            throw new BadRequestException(
                "An account with this email already exists.");
        }

        var existingWhitelist =
            await _emailWhitelistRepository.FindByEmailAsync(email, ct);

        if (existingWhitelist is not null)
        {
            throw new BadRequestException(
                "This email is already whitelisted.");
        }

        var role = await _roleRepository.FindByNameAsync(
            roleName,
            ct);

        if (role is null)
        {
            throw new NotFoundException(
                $"Role '{roleName}' was not found.");
        }

        var whitelist = new EmailWhitelist
        {
            Email = email,
            RoleId = role.Id,
            Role = role,
            IsUsed = false,
            CreatedBy = createdBy,
            Note = string.IsNullOrWhiteSpace(request.Note)
                ? null
                : request.Note.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _emailWhitelistRepository.AddAsync(whitelist, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}