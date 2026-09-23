using StudentPortal.Common.DTOs.EmailWhitelist;

namespace StudentPortal.Service.Interfaces;

public interface IEmailWhitelistService
{
    Task CreateAsync(
        CreateEmailWhitelistRequest request,
        Guid createdBy,
        CancellationToken ct = default);
}