using StayEase.Domain.Entities;

namespace StayEase.Application.Interfaces;

public interface IHotelService
{
    Task<IReadOnlyList<Hotel>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Hotel?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
