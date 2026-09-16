using Core.Enums;

namespace Core.DTOs.Client
{
    internal class ClientDto
    {
        public int Id { get; set; }

        public string CompanyName { get; set; } = string.Empty;
        public string ContactFullName { get; set; } = string.Empty;
        public string? ContactJobTitle { get; set; }

        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Industry { get; set; } = string.Empty;

        public ClientStatus Status { get; set; }
        public DateTime? WorkingSince { get; set; }
        public DateTime? WorkingUntil { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
