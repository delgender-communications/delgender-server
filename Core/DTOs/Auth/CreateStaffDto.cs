using Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace Core.DTOs.Auth
{
    public class CreateStaffDto
    {
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Surname { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string JobTitle { get; set; } = string.Empty;

        [Required]
        public StaffRole Role { get; set; } = StaffRole.Staff;
    }
}
