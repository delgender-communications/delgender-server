namespace Core.DTOs.Auth
{
    public class ResendOtpResultDto
    {
        // how long the UI should disable the resend button for 
        public int CooldownSeconds { get; set; }
    }
}
