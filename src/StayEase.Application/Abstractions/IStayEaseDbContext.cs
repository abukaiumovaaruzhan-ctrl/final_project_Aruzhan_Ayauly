using Microsoft.EntityFrameworkCore;
using StayEase.Domain.Entities;

namespace StayEase.Application.Abstractions;

public interface IStayEaseDbContext
{
    DbSet<Hotel> Hotels { get; }
    DbSet<Room> Rooms { get; }
    DbSet<Booking> Bookings { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
