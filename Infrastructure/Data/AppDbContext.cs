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
        public DbSet<Feedback> Feedbacks => Set<Feedback>();
        public DbSet<OutreachCompany> OutreachCompanies => Set<OutreachCompany>();
        public DbSet<OutreachLog> OutreachLogs => Set<OutreachLog>();
        public DbSet<ResearchLead> ResearchLeads => Set<ResearchLead>();
        public DbSet<ResearchLeadEvidence> ResearchLeadEvidenceItems => Set<ResearchLeadEvidence>();

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

            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.Customer)
                .WithMany(c => c.FeedbackRequests)
                .HasForeignKey(f => f.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.RequestedByStaff)
                .WithMany()
                .HasForeignKey(f => f.RequestedByStaffId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Feedback>()
                .HasIndex(f => f.RequestToken)
                .IsUnique();

            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.Email);

            modelBuilder.Entity<OutreachCompany>()
                .HasOne(o => o.AddedByStaff)
                .WithMany()
                .HasForeignKey(o => o.AddedByStaffId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OutreachCompany>()
                .HasIndex(o => o.Status);

            modelBuilder.Entity<OutreachLog>()
                .HasOne(l => l.OutreachCompany)
                .WithMany(o => o.Logs)
                .HasForeignKey(l => l.OutreachCompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OutreachLog>()
                .HasOne(l => l.Staff)
                .WithMany()
                .HasForeignKey(l => l.StaffId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<OutreachLog>()
                .HasIndex(l => l.CreatedAt);

            modelBuilder.Entity<ResearchLead>()
                .HasIndex(l => l.Status);

            modelBuilder.Entity<ResearchLead>()
                .HasIndex(l => l.NameKey);

            modelBuilder.Entity<ResearchLead>()
                .HasIndex(l => l.WebsiteHost);

            modelBuilder.Entity<ResearchLead>()
                .HasOne(l => l.AddedByStaff)
                .WithMany()
                .HasForeignKey(l => l.AddedByStaffId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ResearchLead>()
                .HasOne(l => l.ReviewedByStaff)
                .WithMany()
                .HasForeignKey(l => l.ReviewedByStaffId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ResearchLead>()
                .HasOne(l => l.OutreachCompany)
                .WithMany()
                .HasForeignKey(l => l.OutreachCompanyId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ResearchLeadEvidence>()
                .HasOne(e => e.ResearchLead)
                .WithMany(l => l.Evidence)
                .HasForeignKey(e => e.ResearchLeadId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
