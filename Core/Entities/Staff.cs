using Core.Enums;

namespace Core.Entities
{
    public class Staff
    {
        public int Id { get; set; }

        public string StaffId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Surname { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string JobTitle { get; set; } = null!;
        public StaffRole Role { get; set; } = StaffRole.Staff;
        public bool IsActive { get; set; } = true;
        public string? ProfilePictureUrl { get; set; }
        public string? ProfilePicturePublicId { get; set; }

        public string PasswordHash { get; set; } = null!;
        public bool MustChangePassword { get; set; } = true;
        public DateTime? LastLoginAt { get; set; }

        public bool NotifyNewLeads { get; set; } = true;
        public bool NotifyNewBookings { get; set; } = true;
        public bool NotifyOverdueInvoices { get; set; } = true;

        public DateTime? NotificationsSeenAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public ICollection<TrustedDevice> TrustedDevices { get; set; } = new List<TrustedDevice>();
        public ICollection<LoginOtp> LoginOtps { get; set; } = new List<LoginOtp>();
    }
}
