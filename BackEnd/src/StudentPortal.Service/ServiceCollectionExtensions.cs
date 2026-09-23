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
<<<<<<< HEAD
        services.AddScoped<IAuditLogService, AuditLogService>();
=======
        services.AddScoped<IProfileService, ProfileService>();
>>>>>>> 618c8528049b691a41dfd66fbf58e4dedcf4c8d6

        return services;
    }
}
