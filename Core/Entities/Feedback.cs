namespace Core.Entities
{
    public class Feedback
    {
        /// <summary>
        /// a record is created when a staff member ends collab with a customer
        /// and opts to send a feedback request to customer
        /// </summary>
        public int Id { get; set; }

        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;

        public Guid RequestToken { get; set; } = Guid.NewGuid();

        public int? RequestedByStaffId { get; set; }
        public Staff? RequestedByStaff { get; set; }
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public bool Submitted { get; set; } = false;
        public int? Rating { get; set; }
        public string? Comments { get; set; }
        public DateTime? SubmittedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
