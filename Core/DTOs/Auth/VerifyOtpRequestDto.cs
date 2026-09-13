using System.ComponentModel.DataAnnotations;

namespace Core.DTOs.Auth
{
    public class VerifyOtpRequestDto
    {
        [Required]
        public string PendingToken { get; set; } = string.Empty;

        [Required, StringLength(6, MinimumLength = 6)]
        public string Code { get; set; } = string.Empty;

        public bool RememberMe { get; set; } = false;
        public bool TrustDevice { get; set; } = false; // "don't ask for OTP on this device" going forward
    }
}
