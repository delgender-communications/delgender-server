using System.ComponentModel.DataAnnotations;

namespace Core.DTOs.Auth
{
    public class ResendOtpRequestDto
    {
        [Required]
        public string PendingToken { get; set; } = string.Empty;
    }
}
