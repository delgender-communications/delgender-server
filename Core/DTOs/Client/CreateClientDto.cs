using Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace Core.DTOs.Client
{
    internal class CreateClientDto
    {
        [Required, StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? JobTitle { get; set; }

        [Required, StringLength(100)]
        public string CompanyName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(10)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Industry { get; set; } = string.Empty;

        [Required]
        public ClientStatus Status { get; set; } = ClientStatus.Pending;

        public bool ContactPermission { get; set; } = false;
    }
}
