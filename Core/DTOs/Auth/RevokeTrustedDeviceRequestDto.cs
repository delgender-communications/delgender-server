namespace Core.DTOs.Auth
{
    public class RevokeTrustedDeviceRequestDto
    {
        public string? RefreshToken { get; set; }
        public string? DeviceToken { get; set; }
    }
}
