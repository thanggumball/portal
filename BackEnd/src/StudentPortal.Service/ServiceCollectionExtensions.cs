using Microsoft.Extensions.DependencyInjection;
using StudentPortal.Repository;
using StudentPortal.Repository.Implementations;
using StudentPortal.Repository.Interfaces;
using StudentPortal.Service.Helpers;
using StudentPortal.Service.Implementations;
using StudentPortal.Service.Interfaces;

namespace StudentPortal.Service;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServiceLayer(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddRepositoryLayer(connectionString);
        services.AddScoped<IAccountSequenceRepository, AccountSequenceRepository>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEmailWhitelistService, EmailWhitelistService>();
        services.AddScoped<IJwtTokenHelper, JwtTokenHelper>();
        services.AddScoped<IProfileService, ProfileService>();

        return services;
    }
}
