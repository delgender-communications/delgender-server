namespace Core.DTOs.Auth
{
    public class AuthTokensDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public DateTime AccessTokenExpiresAt { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime RefreshTokenExpiresAt { get; set; }
        public string? DeviceToken { get; set; } // only present the first time a device is trusted (or re-trusted)
        public DateTime? DeviceTokenExpiresAt { get; set; }
    }
}
