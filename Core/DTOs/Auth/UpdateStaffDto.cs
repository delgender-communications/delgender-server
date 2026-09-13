using System.ComponentModel.DataAnnotations;

namespace Core.DTOs.Auth
{
    public class UpdateStaffDto
    {
        [StringLength(100)]
        public string? Name { get; set; }

        [StringLength(100)]
        public string? Surname { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(100)]
        public string? JobTitle { get; set; }

        public StaffRole? Role { get; set; }
        public bool? IsActive { get; set; }
    }
}
