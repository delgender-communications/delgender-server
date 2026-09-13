using System.ComponentModel.DataAnnotations;

namespace Core.DTOs.Auth
{
    public class LoginRequestDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; } = false;
        public string? DeviceToken { get; set; } // present if the browser previously stored a trusted-device token
    }
}
