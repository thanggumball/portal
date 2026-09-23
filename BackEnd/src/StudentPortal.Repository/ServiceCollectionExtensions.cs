using Audit.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StudentPortal.Repository.Auditing;
using StudentPortal.Repository.Data;
using StudentPortal.Repository.Implementations;
using StudentPortal.Repository.Interfaces;

namespace StudentPortal.Repository;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRepositoryLayer(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options => options
           .UseSqlServer(connectionString)
           // Built NEW inside the lambda: one interceptor per DbContext, because the
           // interceptor holds state tied to that specific context
           .AddInterceptors(new AuditSaveChangesInterceptor()));
        AuditConfiguration.Configure(connectionString);
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IAnnouncementRepository, AnnouncementRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IEmailWhitelistRepository, EmailWhitelistRepository>();
        services.AddScoped<IProfileRepository, ProfileRepository>();
        return services;
    }
}
