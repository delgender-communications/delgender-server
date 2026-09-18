using Core.DTOs.Auth;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace DelgenderCommunicationsAPI.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [EnableRateLimiting("booking")]
        public async Task<ActionResult<LoginResultDto>> Login([FromBody] LoginRequestDto dto)
        {
            var result = await _authService.LoginAsync(dto, GetIp());
            return Ok(result);
        }

        [HttpPost("verify-otp")]
        [AllowAnonymous]
        [EnableRateLimiting("booking")]
        public async Task<ActionResult<VerifyOtpResultDto>> VerifyOtp([FromBody] VerifyOtpRequestDto dto)
        {
            var result = await _authService.VerifyOtpAsync(dto, GetIp(), GetUserAgent());
            return Ok(result);
        }

        [HttpPost("resend-otp")]
        [AllowAnonymous]
        [EnableRateLimiting("booking")]
        public async Task<ActionResult<ResendOtpResultDto>> ResendOtp([FromBody] ResendOtpRequestDto dto)
        {
            var result = await _authService.ResendOtpAsync(dto.PendingToken);
            return Ok(result);
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthTokensDto>> Refresh([FromBody] RefreshRequestDto dto)
        {
            var tokens = await _authService.RefreshAsync(dto.RefreshToken, GetIp());
            return Ok(tokens);
        }

        [HttpPost("logout")]
        [AllowAnonymous]
        public async Task<IActionResult> Logout([FromBody] LogoutRequestDto dto)
        {
            await _authService.LogoutAsync(dto.RefreshToken);
            return NoContent();
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            await _authService.ChangePasswordAsync(CurrentStaffId(), dto);
            return NoContent();
        }

        [HttpGet("trusted-devices")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<TrustedDeviceDto>>> GetTrustedDevices(
            [FromQuery] string? deviceToken = null)
        {
            var devices = await _authService.GetTrustedDevicesAsync(CurrentStaffId(), deviceToken);
            return Ok(devices);
        }

        [HttpDelete("trusted-devices/{id:int}")]
        [Authorize]
        public async Task<IActionResult> RevokeTrustedDevice(int id)
        {
            await _authService.RevokeTrustedDeviceAsync(CurrentStaffId(), id);
            return NoContent();
        }

        private int CurrentStaffId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private string? GetIp() =>
            HttpContext.Connection.RemoteIpAddress?.ToString();

        private string? GetUserAgent() =>
            Request.Headers.UserAgent.ToString();
    }
}
