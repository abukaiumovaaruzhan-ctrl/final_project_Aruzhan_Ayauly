
using Microsoft.EntityFrameworkCore;
using StayEase.Domain.Entities;

namespace StayEase.Infrastructure.Data;

public class StayEaseDbContext : DbContext
{
    public StayEaseDbContext(
        DbContextOptions<StayEaseDbContext> options)
        : base(options)
    {
    }

    public DbSet<Hotel> Hotels => Set<Hotel>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Hotel>().ToTable("hotels");
        modelBuilder.Entity<Room>().ToTable("rooms");
        modelBuilder.Entity<Booking>().ToTable("bookings");
        modelBuilder.Entity<Payment>().ToTable("payments");

        modelBuilder.Entity<Room>()
            .HasOne<Hotel>()
            .WithMany(h => h.Rooms)
            .HasForeignKey(r => r.HotelId);

        modelBuilder.Entity<Booking>()
            .HasOne<Room>()
            .WithMany()
            .HasForeignKey(b => b.RoomId);

        modelBuilder.Entity<Payment>()
            .HasOne<Booking>()
            .WithMany()
            .HasForeignKey(p => p.BookingId);

        modelBuilder.Entity<Room>()
            .Property(r => r.PricePerNight)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Booking>()
            .Property(b => b.TotalPrice)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Payment>()
            .Property(p => p.Amount)
            .HasPrecision(10, 2);
    }
}
