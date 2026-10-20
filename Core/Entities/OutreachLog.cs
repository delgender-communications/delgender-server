namespace Core.Entities
{
    public class OutreachLog
    {
        public int Id { get; set; }

        public int OutreachCompanyId { get; set; }
        public OutreachCompany OutreachCompany { get; set; } = null!;

        public int? StaffId { get; set; }
        public Staff? Staff { get; set; }

        public string ActorName { get; set; } = null!;
        public string Message { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
