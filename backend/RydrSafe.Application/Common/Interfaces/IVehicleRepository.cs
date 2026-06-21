using RydrSafe.Domain.Entities;

namespace RydrSafe.Application.Common.Interfaces;

public interface IVehicleRepository
{
    Task<Vehicle?> GetByRegistrationNumberAsync(string registrationNumber);
    Task<IEnumerable<Vehicle>> GetByDriverIdAsync(Guid driverId);
    Task AddAsync(Vehicle vehicle);
}
