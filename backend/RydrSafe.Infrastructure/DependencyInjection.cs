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
        services.AddScoped<IVerificationHistoryRepository, VerificationHistoryRepository>();
        services.AddScoped<IDriverFollowRepository, DriverFollowRepository>();
        services.AddScoped<IConsentRepository, ConsentRepository>();
        services.AddScoped<IAuditRepository, AuditRepository>();
        services.AddScoped<IAppealRepository, AppealRepository>();
        services.AddScoped<IDriverAccessLogRepository, DriverAccessLogRepository>();
        services.AddScoped<IRecommendationRepository, RecommendationRepository>();

        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddHttpClient<IOcrService, OcrService>();
        services.AddScoped<IRiskScoringService, RiskScoringService>();
        services.AddScoped<ICorroborationService, CorroborationService>();
        services.AddScoped<IRealtimeNotificationService, SignalRNotificationService>();

        // Singletons: both read configuration once and hold no per-request state.
        services.AddSingleton<ICategoryAGate, CategoryAGate>();
        services.AddSingleton<IHashingService, HashingService>();
        services.AddSingleton<IRetentionPolicy, RetentionPolicy>();

        // Clause 29 enforcement (COMPLIANCE-NOTES B12).
        services.AddHostedService<RetentionWorker>();

        return services;
    }
}
