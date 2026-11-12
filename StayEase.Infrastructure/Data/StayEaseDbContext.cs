using Microsoft.EntityFrameworkCore;
using StayEase.Domain.Entities;

namespace StayEase.Infrastructure.Data
{
    public class StayEaseDbContext : DbContext
    {
        public StayEaseDbContext(DbContextOptions<StayEaseDbContext> options) : base(options)
        {
        }

    
        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

      
            modelBuilder.Entity<Hotel>()
                .HasMany(h => h.Rooms)
                .WithOne() 
                .HasForeignKey("HotelId");
        }
    }
}