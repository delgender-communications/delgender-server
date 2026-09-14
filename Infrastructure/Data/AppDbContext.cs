using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Booking> Bookings => Set<Booking>();
        public DbSet<Confirmation> Confirmations => Set<Confirmation>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Invoice> Invoices => Set<Invoice>();
        public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
        public DbSet<PageView> PageViews => Set<PageView>();
        public DbSet<Staff> Staffs => Set<Staff>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<TrustedDevice> TrustedDevices => Set<TrustedDevice>();
        public DbSet<LoginOtp> LoginOtps => Set<LoginOtp>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Confirmation)
                .WithOne(c => c.Booking)
                .HasForeignKey<Confirmation>(c => c.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Customer)
                .WithMany(c => c.Bookings)
                .HasForeignKey(b => b.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Confirmation)
                .WithOne(c => c.Booking)
                .HasForeignKey<Confirmation>(c => c.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Customer)
                .WithMany(c => c.Invoices)
                .HasForeignKey(i => i.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InvoiceItem>()
                .HasOne(ii => ii.Invoice)
                .WithMany(i => i.Items)
                .HasForeignKey(ii => ii.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Invoice>()
                .HasIndex(i => i.InvoiceNumber)
                .IsUnique();

            modelBuilder.Entity<Staff>()
                .HasIndex(s => s.Email)
                .IsUnique();

            modelBuilder.Entity<Staff>()
                .HasIndex(s => s.StaffId)
                .IsUnique();

            modelBuilder.Entity<Booking>()
                .HasIndex(b => b.BookingReference)
                .IsUnique();

            modelBuilder.Entity<Booking>()
                .HasIndex(b => b.BookingNumber)
                .IsUnique();

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.RespondedByStaff)
                .WithMany()
                .HasForeignKey(b => b.RespondedByStaffId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Booking)
                .WithMany(b => b.Invoices)
                .HasForeignKey(i => i.BookingId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.CreatedByStaff)
                .WithMany()
                .HasForeignKey(i => i.CreatedByStaffId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<RefreshToken>()
                .HasOne(r => r.Staff)
                .WithMany(s => s.RefreshTokens)
                .HasForeignKey(r => r.StaffId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(r => r.TokenHash)
                .IsUnique();

            modelBuilder.Entity<TrustedDevice>()
                .HasOne(t => t.Staff)
                .WithMany(s => s.TrustedDevices)
                .HasForeignKey(t => t.StaffId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TrustedDevice>()
                .HasIndex(t => t.TokenHash)
                .IsUnique();

            modelBuilder.Entity<LoginOtp>()
                .HasOne(o => o.Staff)
                .WithMany(s => s.LoginOtps)
                .HasForeignKey(o => o.StaffId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PageView>()
                .HasIndex(p => p.VisitedAt);

            modelBuilder.Entity<PageView>()
                .HasIndex(p => p.VisitorId);
        }
    }
}
