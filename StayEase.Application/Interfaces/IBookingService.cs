using StayEase.Domain.Entities;

namespace StayEase.Application.Interfaces;

public interface IBookingService
{
    Task<IReadOnlyList<Booking>> GetCustomerBookingsAsync(
        string customerId, CancellationToken cancellationToken = default);

    Task<bool> IsRoomAvailableAsync(
        int roomId, DateOnly checkIn, DateOnly checkOut,
        CancellationToken cancellationToken = default);
}
