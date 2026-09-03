using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Booking> Bookings => Set<Booking>();
        public DbSet<Confirmation> Confirmations => Set<Confirmation>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Confirmation)
                .WithOne(c => c.Booking)
                .HasForeignKey<Confirmation>(c => c.BookingId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
