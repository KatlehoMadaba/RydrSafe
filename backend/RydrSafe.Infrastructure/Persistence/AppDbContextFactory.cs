using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RydrSafe.Infrastructure.Persistence;

/// <summary>
/// Used by <c>dotnet ef</c> only. Without it, EF boots the API host to find the context, which
/// runs the startup migration against whatever database the local configuration points at —
/// not something a developer should trigger by scaffolding a migration.
///
/// The connection string here is never connected to; EF needs a provider to generate
/// provider-specific SQL, nothing more.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("RYDRSAFE_DESIGN_CONNECTION")
            ?? "Host=localhost;Database=rydrsafe_design;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new AppDbContext(options);
    }
}
