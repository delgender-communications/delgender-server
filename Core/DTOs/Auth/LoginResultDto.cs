namespace Core.DTOs.Auth
{
    public class LoginResultDto
    {
        public bool RequiresOtp { get; set; } // true when an OTP email was sent and must be verified before tokens are issued
        public string? PendingToken { get; set; } // temporary token identifying the in-progress login; required by /verify-otp

        public AuthTokensDto? Tokens { get; set; }
        public StaffDto? Staff { get; set; }
    }
}
