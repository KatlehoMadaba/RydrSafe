using Microsoft.EntityFrameworkCore;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Domain.Entities;

namespace RydrSafe.Infrastructure.Persistence.Repositories;

public class DriverRepository(AppDbContext db) : IDriverRepository
{
    public async Task<Driver?> GetByIdAsync(Guid id) =>
        await db.Drivers.Include(d => d.Vehicles).Include(d => d.Reports).FirstOrDefaultAsync(d => d.Id == id);

    public async Task<IEnumerable<Driver>> GetAllAsync(int page, int pageSize, string? search = null) =>
        await ApplySearch(db.Drivers.Include(d => d.Vehicles), search)
            .OrderByDescending(d => d.RiskScore)
            // Tiebreaker: without one, drivers sharing a risk score have no defined order
            // between queries, so paging can show the same row twice and skip another.
            .ThenBy(d => d.Id)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync();

    public async Task<IEnumerable<Driver>> SearchAsync(string query) =>
        await ApplySearch(db.Drivers.Include(d => d.Vehicles), query)
            .OrderByDescending(d => d.RiskScore)
            .ThenBy(d => d.Id)
            .ToListAsync();

    /// <summary>
    /// The single definition of "matches this search": driver name, any of their registration
    /// numbers, or phone. The paged list, its count and the standalone search endpoint all go
    /// through here, so a filtered page and its total cannot disagree about what matched.
    ///
    /// <c>ILike</c> rather than <c>ToLower().Contains()</c> — it is one case-insensitive
    /// comparison in Postgres instead of lowering every row, and a pg_trgm GIN index can serve
    /// it later. Today it is still a sequential scan; that is fine at this table size and is
    /// the thing to revisit before the driver list grows.
    /// </summary>
    private static IQueryable<Driver> ApplySearch(IQueryable<Driver> drivers, string? search)
    {
        if (string.IsNullOrWhiteSpace(search)) return drivers;

        // Escape LIKE wildcards before interpolating. Without this a search for "%" matches
        // every driver and "_" matches any single character — user input, read as a pattern.
        var escaped = search.Trim()
            .Replace("\\", "\\\\")
            .Replace("%", "\\%")
            .Replace("_", "\\_");

        var pattern = $"%{escaped}%";

        return drivers.Where(d =>
            EF.Functions.ILike(d.DriverName, pattern, "\\")
            || d.Vehicles.Any(v => EF.Functions.ILike(v.RegistrationNumber, pattern, "\\"))
            || (d.PhoneNumber != null && EF.Functions.ILike(d.PhoneNumber, pattern, "\\")));
    }

    public async Task<Driver?> GetByRegistrationNumberAsync(string registrationNumber)
    {
        var vehicle = await db.Vehicles
            .FirstOrDefaultAsync(v => v.RegistrationNumber.ToLower() == registrationNumber.ToLower());
        if (vehicle is null) return null;
        return await db.Drivers.Include(d => d.Vehicles).FirstOrDefaultAsync(d => d.Id == vehicle.DriverId);
    }

    public async Task<Driver?> GetByPhoneNumberAsync(string phoneNumber) =>
        await db.Drivers.Include(d => d.Vehicles)
            .FirstOrDefaultAsync(d => d.PhoneNumber == phoneNumber);

    public async Task<IEnumerable<Driver>> GetByNameFuzzyAsync(string name)
    {
        var all = await db.Drivers.Include(d => d.Vehicles).ToListAsync();
        return all.Where(d => LevenshteinDistance(d.DriverName.ToLower(), name.ToLower()) <= 2).ToList();
    }

    public async Task AddAsync(Driver driver)
    {
        db.Drivers.Add(driver);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Driver driver)
    {
        db.Drivers.Update(driver);
        await db.SaveChangesAsync();
    }

    public async Task<int> CountAsync(string? search = null) =>
        await ApplySearch(db.Drivers, search).CountAsync();

    public async Task<int> CountByStatusAsync(params RydrSafe.Domain.Enums.DriverStatus[] statuses) =>
        await db.Drivers.CountAsync(d => statuses.Contains(d.Status));

    private static int LevenshteinDistance(string a, string b)
    {
        int[,] dp = new int[a.Length + 1, b.Length + 1];
        for (int i = 0; i <= a.Length; i++) dp[i, 0] = i;
        for (int j = 0; j <= b.Length; j++) dp[0, j] = j;
        for (int i = 1; i <= a.Length; i++)
            for (int j = 1; j <= b.Length; j++)
                dp[i, j] = a[i - 1] == b[j - 1]
                    ? dp[i - 1, j - 1]
                    : 1 + Math.Min(dp[i - 1, j - 1], Math.Min(dp[i - 1, j], dp[i, j - 1]));
        return dp[a.Length, b.Length];
    }
}
