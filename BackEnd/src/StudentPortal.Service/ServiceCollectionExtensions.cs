using Microsoft.Extensions.DependencyInjection;
using StudentPortal.Repository;
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

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtTokenHelper, JwtTokenHelper>();

        return services;
    }
}