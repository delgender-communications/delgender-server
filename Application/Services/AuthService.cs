using Core.Configuration;
using Core.DTOs.Auth;
using Core.Entities;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IStaffRepository _staffRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ITrustedDeviceRepository _trustedDeviceRepository;
        private readonly ILoginOtpRepository _loginOtpRepository;
        private readonly IEmailService _emailService;
        private readonly JwtSettings _jwtSettings;

        public AuthService(
            IStaffRepository staffRepository,
            IRefreshTokenRepository refreshTokenRepository,
            ITrustedDeviceRepository trustedDeviceRepository,
            ILoginOtpRepository loginOtpRepository,
            IEmailService emailService,
            IOptions<JwtSettings> jwtSettings)
        {
            _staffRepository = staffRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _trustedDeviceRepository = trustedDeviceRepository;
            _loginOtpRepository = loginOtpRepository;
            _emailService = emailService;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<LoginResultDto> LoginAsync(LoginRequestDto dto, string? ipAddress)
        {
            var staff = await _staffRepository.GetByEmailAsync(dto.Email);

            if (staff is null || !BCrypt.Net.BCrypt.Verify(dto.Password, staff.PasswordHash))
            {
                throw new UnauthorizedAccessException("Incorrect email or password.");
            }

            if (!staff.IsActive)
            {
                throw new UnauthorizedAccessException("This account has been deactivated. Contact an administrator.");
            }

            if (!string.IsNullOrWhiteSpace(dto.DeviceToken))
            {
                var deviceHash = HashToken(dto.DeviceToken);
                var device = await _trustedDeviceRepository.GetByTokenHashAsync(deviceHash);

                if (device is not null && device.StaffId == staff.Id &&
                    device.RevokedAt is null && device.ExpiresAt > DateTime.UtcNow)
                {
                    device.LastUsedAt = DateTime.UtcNow;
                    await _trustedDeviceRepository.UpdateAsync(device);

                    staff.LastLoginAt = DateTime.UtcNow;
                    await _staffRepository.UpdateAsync(staff);

                    var directTokens = await GenerateTokensAsync(staff, dto.RememberMe, ipAddress);

                    return new LoginResultDto
                    {
                        RequiresOtp = false,
                        Tokens = directTokens,
                        Staff = ToDto(staff)
                    };
                }
            }

            var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

            var otp = new LoginOtp
            {
                StaffId = staff.Id,
                CodeHash = HashToken(code),
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.OtpExpiryMinutes)
            };

            await _loginOtpRepository.CreateAsync(otp);

            await _emailService.SendOtpAsync(staff.Email, staff.Name, code, _jwtSettings.OtpExpiryMinutes);

            var pendingToken = GeneratePendingToken(staff.Id);

            return new LoginResultDto
            {
                RequiresOtp = true,
                PendingToken = pendingToken
            };
        }

        public async Task<VerifyOtpResultDto> VerifyOtpAsync(VerifyOtpRequestDto dto, string? ipAddress)
        {
            var staffId = ReadPendingToken(dto.PendingToken);
            var staff = await _staffRepository.GetByIdAsync(staffId)
                ?? throw new UnauthorizedAccessException("Login session expired. Please log in again.");

            var otp = await _loginOtpRepository.GetLatestActiveForStaffAsync(staffId)
                ?? throw new UnauthorizedAccessException("This code has expired. Please request a new one.");

            if (otp.Attempts >= _jwtSettings.OtpMaxAttempts)
            {
                throw new UnauthorizedAccessException("Too many incorrect attempts. Please request a new code.");
            }

            if (HashToken(dto.Code) != otp.CodeHash)
            {
                otp.Attempts += 1;
                await _loginOtpRepository.UpdateAsync(otp);
                throw new UnauthorizedAccessException("That code is incorrect.");
            }

            otp.ConsumedAt = DateTime.UtcNow;
            await _loginOtpRepository.UpdateAsync(otp);

            var tokens = await GenerateTokensAsync(staff, dto.RememberMe, ipAddress);

            if (dto.TrustDevice)
            {
                var rawDeviceToken = GenerateOpaqueToken();
                var expiresAt = DateTime.UtcNow.AddDays(_jwtSettings.TrustedDeviceDays);

                await _trustedDeviceRepository.CreateAsync(new TrustedDevice
                {
                    StaffId = staff.Id,
                    TokenHash = HashToken(rawDeviceToken),
                    ExpiresAt = expiresAt
                });

                tokens.DeviceToken = rawDeviceToken;
                tokens.DeviceTokenExpiresAt = expiresAt;
            }

            staff.LastLoginAt = DateTime.UtcNow;
            await _staffRepository.UpdateAsync(staff);

            return new VerifyOtpResultDto
            {
                Tokens = tokens,
                Staff = ToDto(staff)
            };
        }

        public async Task<AuthTokensDto> RefreshAsync(string refreshToken, string? ipAddress)
        {
            var hash = HashToken(refreshToken);
            var existing = await _refreshTokenRepository.GetByTokenHashAsync(hash);

            if (existing is null || !existing.IsActive)
            {
                throw new UnauthorizedAccessException("Session expired. Please log in again.");
            }

            existing.RevokedAt = DateTime.UtcNow;

            var newTokens = await GenerateTokensAsync(existing.Staff, existing.RememberMe, ipAddress);

            existing.ReplacedByTokenHash = HashToken(newTokens.RefreshToken);
            await _refreshTokenRepository.UpdateAsync(existing);

            return newTokens;
        }

        public async Task LogoutAsync(string refreshToken)
        {
            var hash = HashToken(refreshToken);
            var existing = await _refreshTokenRepository.GetByTokenHashAsync(hash);

            if (existing is not null && existing.RevokedAt is null)
            {
                existing.RevokedAt = DateTime.UtcNow;
                await _refreshTokenRepository.UpdateAsync(existing);
            }
        }

        public async Task ChangePasswordAsync(int staffId, ChangePasswordDto dto)
        {
            var staff = await _staffRepository.GetByIdAsync(staffId)
                ?? throw new KeyNotFoundException("Staff member not found.");

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, staff.PasswordHash))
            {
                throw new UnauthorizedAccessException("Current password is incorrect.");
            }

            staff.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            staff.MustChangePassword = false;
            staff.UpdatedAt = DateTime.UtcNow;

            await _staffRepository.UpdateAsync(staff);
        }

        public async Task<IEnumerable<TrustedDeviceDto>> GetTrustedDevicesAsync(int staffId)
        {
            var devices = await _trustedDeviceRepository.GetActiveForStaffAsync(staffId);

            return devices.Select(d => new TrustedDeviceDto
            {
                Id = d.Id,
                Label = d.Label,
                CreatedAt = d.CreatedAt,
                LastUsedAt = d.LastUsedAt,
                ExpiresAt = d.ExpiresAt
            });
        }

        public async Task RevokeTrustedDeviceAsync(int staffId, int deviceId)
        {
            var device = await _trustedDeviceRepository.GetByIdAsync(deviceId);

            if (device is null || device.StaffId != staffId)
            {
                throw new KeyNotFoundException("Trusted device not found.");
            }

            device.RevokedAt = DateTime.UtcNow;
            await _trustedDeviceRepository.UpdateAsync(device);
        }

        private async Task<AuthTokensDto> GenerateTokensAsync(Staff staff, bool rememberMe, string? ipAddress)
        {
            var accessToken = GenerateAccessToken(staff);
            var accessExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenMinutes);

            var refreshRaw = GenerateOpaqueToken();
            var refreshDays = rememberMe ? _jwtSettings.RefreshTokenDaysRemembered : _jwtSettings.RefreshTokenDaysSession;
            var refreshExpiresAt = DateTime.UtcNow.AddDays(refreshDays);

            await _refreshTokenRepository.CreateAsync(new RefreshToken
            {
                StaffId = staff.Id,
                TokenHash = HashToken(refreshRaw),
                RememberMe = rememberMe,
                CreatedByIp = ipAddress,
                ExpiresAt = refreshExpiresAt
            });

            return new AuthTokensDto
            {
                AccessToken = accessToken,
                AccessTokenExpiresAt = accessExpiresAt,
                RefreshToken = refreshRaw,
                RefreshTokenExpiresAt = refreshExpiresAt
            };
        }

        private string GenerateAccessToken(Staff staff)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, staff.Id.ToString()),
                new(ClaimTypes.NameIdentifier, staff.Id.ToString()),
                new(ClaimTypes.Email, staff.Email),
                new(ClaimTypes.Name, $"{staff.Name} {staff.Surname}"),
                new(ClaimTypes.Role, staff.Role.ToString()),
                new("staffCode", staff.StaffId)
            };

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GeneratePendingToken(int staffId)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, staffId.ToString()),
                new("purpose", "otp")
            };

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.OtpExpiryMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private int ReadPendingToken(string pendingToken)
        {
            var handler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));

            try
            {
                var principal = handler.ValidateToken(pendingToken, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30)
                }, out _);

                var purpose = principal?.FindFirst("purpose")?.Value;
                if (purpose != "otp")
                {
                    throw new UnauthorizedAccessException("Invalid login session.");
                }

                return int.Parse(principal?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? throw new InvalidOperationException("Invalid token."));
            }
            catch (Exception)
            {
                throw new UnauthorizedAccessException("Login session expired. Please log in again.");
            }
        }

        private static string GenerateOpaqueToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(bytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }

        private static string HashToken(string value)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
            return Convert.ToHexString(bytes);
        }

        private static StaffDto ToDto(Staff staff) => new()
        {
            Id = staff.Id,
            StaffId = staff.StaffId,
            Name = staff.Name,
            Surname = staff.Surname,
            Email = staff.Email,
            PhoneNumber = staff.PhoneNumber,
            JobTitle = staff.JobTitle,
            Role = staff.Role,
            IsActive = staff.IsActive,
            MustChangePassword = staff.MustChangePassword,
            ProfilePictureUrl = staff.ProfilePictureUrl,
            LastLoginAt = staff.LastLoginAt,
            CreatedAt = staff.CreatedAt
        };
    }
}
