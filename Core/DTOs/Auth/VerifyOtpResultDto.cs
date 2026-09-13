namespace Core.DTOs.Auth
{
    public class VerifyOtpResultDto
    {
        public AuthTokensDto Tokens { get; set; } = null!;
        public StaffDto Staff { get; set; } = null!;
    }
}
