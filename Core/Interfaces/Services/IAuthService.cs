using Core.DTOs.Auth;

namespace Core.Interfaces.Services
{
    public interface IAuthService
    {
        Task<LoginResultDto> LoginAsync(LoginRequestDto dto, string? ipAddress);
        Task<VerifyOtpResultDto> VerifyOtpAsync(VerifyOtpRequestDto dto, string? ipAddress, string? userAgent);
        Task<ResendOtpResultDto> ResendOtpAsync(string pendingToken);
        Task<AuthTokensDto> RefreshAsync(string refreshToken, string? ipAddress);
        Task LogoutAsync(string refreshToken);
        Task ChangePasswordAsync(int staffId, ChangePasswordDto dto);
        Task<IEnumerable<TrustedDeviceDto>> GetTrustedDevicesAsync(int staffId, string? currentDeviceToken);
        Task RevokeTrustedDeviceAsync(int staffId, int deviceId);
    }
}
