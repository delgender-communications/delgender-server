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
        public bool IsActive { get; set; } = true;
        public string? ProfilePictureUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
