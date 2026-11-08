namespace Core.Entities
{
    public class NotificationRead
    {
        public int Id { get; set; }

        public int NotificationId { get; set; }
        public int StaffId { get; set; }

        public DateTime ReadAt { get; set; } = DateTime.UtcNow;
    }
}
