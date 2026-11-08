namespace Core.DTOs.Notifications
{
    public class NotificationPreferencesDto
    {
        public bool NotifyNewLeads { get; set; }
        public bool NotifyNewBookings { get; set; }
        public bool NotifyOverdueInvoices { get; set; }
    }
}
