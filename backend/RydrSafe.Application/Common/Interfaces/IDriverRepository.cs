using RydrSafe.Domain.Entities;

namespace RydrSafe.Application.Common.Interfaces;

public interface IDriverRepository
{
    Task<Driver?> GetByIdAsync(Guid id);
    /// <summary>
    /// A page of drivers, optionally filtered by name, registration or phone. Search is part of
    /// the paged query rather than a separate unpaginated endpoint, so a filtered list behaves
    /// like any other page.
    /// </summary>
    Task<IEnumerable<Driver>> GetAllAsync(int page, int pageSize, string? search = null);

    /// <summary>
    /// Total matching <paramref name="search"/>. Must apply the same predicate as
    /// <see cref="GetAllAsync"/>: a filtered page with an unfiltered total gives the client a
    /// page count for rows it will never be shown.
    /// </summary>
    Task<int> CountAsync(string? search = null);

    Task<IEnumerable<Driver>> SearchAsync(string query);
    Task<Driver?> GetByRegistrationNumberAsync(string registrationNumber);
    Task<Driver?> GetByPhoneNumberAsync(string phoneNumber);
    Task<IEnumerable<Driver>> GetByNameFuzzyAsync(string name);
    Task AddAsync(Driver driver);
    Task UpdateAsync(Driver driver);
    Task<int> CountByStatusAsync(params RydrSafe.Domain.Enums.DriverStatus[] statuses);
}
