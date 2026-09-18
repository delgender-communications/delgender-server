namespace Core.Configuration
{
    public class JwtSettings
    {
        public string Secret { get; set; } = null!;
        public string Issuer { get; set; } = null!;
        public string Audience { get; set; } = null!;
        public int AccessTokenMinutes { get; set; } = 15;
        public int RefreshTokenDaysRemembered { get; set; } = 30;
        public int RefreshTokenDaysSession { get; set; } = 1;
        public int TrustedDeviceDays { get; set; } = 60;
        public int OtpExpiryMinutes { get; set; } = 10;
        public int OtpMaxAttempts { get; set; } = 5;
        public int OtpResendCooldownSeconds { get; set; } = 60;
    }
}
