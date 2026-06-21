using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Infrastructure.Persistence;
using RydrSafe.Infrastructure.Persistence.Repositories;
using RydrSafe.Infrastructure.Services;

namespace RydrSafe.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(config.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IDriverRepository, DriverRepository>();
        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();

        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IOcrService, OcrService>();
        services.AddScoped<IRiskScoringService, RiskScoringService>();
        services.AddScoped<IRealtimeNotificationService, SignalRNotificationService>();

        return services;
    }
}
