using Microsoft.EntityFrameworkCore;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Domain.Entities;

namespace RydrSafe.Infrastructure.Persistence.Repositories;

public class DriverRepository(AppDbContext db) : IDriverRepository
{
    public async Task<Driver?> GetByIdAsync(Guid id) =>
        await db.Drivers.Include(d => d.Vehicles).Include(d => d.Reports).FirstOrDefaultAsync(d => d.Id == id);

    public async Task<IEnumerable<Driver>> GetAllAsync(int page, int pageSize) =>
        await db.Drivers.Include(d => d.Vehicles)
            .OrderByDescending(d => d.RiskScore)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync();

    public async Task<IEnumerable<Driver>> SearchAsync(string query) =>
        await db.Drivers.Include(d => d.Vehicles)
            .Where(d => d.DriverName.ToLower().Contains(query.ToLower())
                || d.Vehicles.Any(v => v.RegistrationNumber.ToLower().Contains(query.ToLower()))
                || (d.PhoneNumber != null && d.PhoneNumber.Contains(query)))
            .ToListAsync();

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

    public async Task<int> CountAsync() => await db.Drivers.CountAsync();

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
