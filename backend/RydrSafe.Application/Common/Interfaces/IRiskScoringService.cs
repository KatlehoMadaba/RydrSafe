using RydrSafe.Domain.Entities;

namespace RydrSafe.Application.Common.Interfaces;

public interface IRiskScoringService
{
    Task<int> CalculateAsync(Guid driverId);
}
