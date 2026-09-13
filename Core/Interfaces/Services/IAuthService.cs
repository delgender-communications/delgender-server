using Core.DTOs.Auth;

namespace Core.Interfaces.Services
{
    public interface IAuthService
    {
        Task<LoginResultDto> LoginAsync(LoginRequestDto dto, string? ipAddress);
        Task<VerifyOtpResultDto> VerifyOtpAsync(VerifyOtpRequestDto dto, string? ipAddress);
        Task<AuthTokensDto> RefreshAsync(string refreshToken, string? ipAddress);
        Task LogoutAsync(string refreshToken);
        Task ChangePasswordAsync(int staffId, ChangePasswordDto dto);
        Task<IEnumerable<TrustedDeviceDto>> GetTrustedDevicesAsync(int staffId);
        Task RevokeTrustedDeviceAsync(int staffId, int deviceId);
    }
}
