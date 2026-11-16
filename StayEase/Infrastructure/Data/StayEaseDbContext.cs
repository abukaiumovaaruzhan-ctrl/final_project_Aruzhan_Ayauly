using Microsoft.EntityFrameworkCore;
using StayEase.Domain;

namespace StayEase.Infrastructure.Data;

public class StayEaseDbContext(DbContextOptions<StayEaseDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Hotel> Hotels => Set<Hotel>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<User>(e =>
        {
            e.ToTable("Users");
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Role).HasConversion<string>();
        });

        mb.Entity<Hotel>(e =>
        {
            e.ToTable("Hotels");
            e.HasMany(h => h.Rooms).WithOne(r => r.Hotel).HasForeignKey(r => r.HotelId).OnDelete(DeleteBehavior.Cascade);
        });

        mb.Entity<Room>(e =>
        {
            e.ToTable("Rooms");
            e.Property(r => r.RoomType).HasConversion<string>();
            e.Property(r => r.Price).HasConversion<double>(); // SQLite не умеет суммировать/сравнивать decimal
        });

        mb.Entity<Booking>(e =>
        {
            e.ToTable("Bookings");
            e.Property(b => b.Status).HasConversion<string>();
            e.Property(b => b.TotalPrice).HasConversion<double>();
            e.HasOne(b => b.Room).WithMany().HasForeignKey(b => b.RoomId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(b => b.User).WithMany().HasForeignKey(b => b.UserId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
