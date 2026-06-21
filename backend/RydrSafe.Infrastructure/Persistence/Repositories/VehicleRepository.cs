using Microsoft.EntityFrameworkCore;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Domain.Entities;

namespace RydrSafe.Infrastructure.Persistence.Repositories;

public class VehicleRepository(AppDbContext db) : IVehicleRepository
{
    public async Task<Vehicle?> GetByRegistrationNumberAsync(string registrationNumber) =>
        await db.Vehicles.FirstOrDefaultAsync(v => v.RegistrationNumber.ToLower() == registrationNumber.ToLower());

    public async Task<IEnumerable<Vehicle>> GetByDriverIdAsync(Guid driverId) =>
        await db.Vehicles.Where(v => v.DriverId == driverId).ToListAsync();

    public async Task AddAsync(Vehicle vehicle)
    {
        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync();
    }
}
