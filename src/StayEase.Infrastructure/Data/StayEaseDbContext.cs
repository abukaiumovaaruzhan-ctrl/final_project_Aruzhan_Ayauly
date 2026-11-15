using Microsoft.EntityFrameworkCore;
using StayEase.Application.Abstractions;
using StayEase.Domain.Entities;

namespace StayEase.Infrastructure.Data;

public class StayEaseDbContext(DbContextOptions<StayEaseDbContext> options) : DbContext(options), IStayEaseDbContext
{
    public DbSet<Hotel> Hotels => Set<Hotel>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Hotel>().Property(x => x.Name).HasMaxLength(120).IsRequired();
        modelBuilder.Entity<Hotel>().Property(x => x.Location).HasMaxLength(160).IsRequired();
        modelBuilder.Entity<Room>().Property(x => x.PricePerNight).HasPrecision(10, 2);
        modelBuilder.Entity<Booking>().Property(x => x.TotalPrice).HasPrecision(10, 2);
        modelBuilder.Entity<Hotel>().HasMany(x => x.Rooms).WithOne(x => x.Hotel).HasForeignKey(x => x.HotelId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Room>().HasMany(x => x.Bookings).WithOne(x => x.Room).HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.Restrict);
    }
}
