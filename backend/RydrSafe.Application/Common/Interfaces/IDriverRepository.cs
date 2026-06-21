using RydrSafe.Domain.Entities;

namespace RydrSafe.Application.Common.Interfaces;

public interface IDriverRepository
{
    Task<Driver?> GetByIdAsync(Guid id);
    Task<IEnumerable<Driver>> GetAllAsync(int page, int pageSize);
    Task<IEnumerable<Driver>> SearchAsync(string query);
    Task<Driver?> GetByRegistrationNumberAsync(string registrationNumber);
    Task<Driver?> GetByPhoneNumberAsync(string phoneNumber);
    Task<IEnumerable<Driver>> GetByNameFuzzyAsync(string name);
    Task AddAsync(Driver driver);
    Task UpdateAsync(Driver driver);
    Task<int> CountAsync();
}
