using Core.DTOs;
using Core.DTOs.Auth;
using Core.Entities;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;

namespace Application.Services
{
    public class StaffService
    {
        private const string ProfilePictureFolder = "delgender-communications/staff";

        private readonly IStaffRepository _staffRepository;
        private readonly IEmailService _emailService;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IConfiguration _configuration;

        public StaffService(
            IStaffRepository staffRepository,
            IEmailService emailService,
            ICloudinaryService cloudinaryService,
            IConfiguration configuration)
        {
            _staffRepository = staffRepository;
            _emailService = emailService;
            _cloudinaryService = cloudinaryService;
            _configuration = configuration;
        }

        public async Task<StaffDto> CreateAsync(CreateStaffDto dto)
        {
            var existing = await _staffRepository.GetByEmailAsync(dto.Email);
            if (existing is not null)
            {
                throw new InvalidOperationException("A staff member with this email already exists.");
            }

            var temporaryPassword = GenerateTemporaryPassword();
            var staffCode = await _staffRepository.GenerateNextStaffCodeAsync();

            var staff = new Staff
            {
                StaffId = staffCode,
                Name = dto.Name,
                Surname = dto.Surname,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                JobTitle = dto.JobTitle,
                Role = dto.Role,
                IsActive = true,
                MustChangePassword = true,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(temporaryPassword)
            };

            staff = await _staffRepository.CreateAsync(staff);

            var loginUrl = _configuration["StaffPortal:LoginUrl"] ?? "https://portal.delgendercommunications.site/login";
            await _emailService.SendStaffWelcomeAsync(staff, temporaryPassword, loginUrl);

            return ToDto(staff);
        }

        public async Task<PagedResultDto<StaffDto>> GetAllAsync(int page, int pageSize)
        {
            var all = (await _staffRepository.GetAllAsync())
                .OrderBy(s => s.Name)
                .ToList();

            var pageItems = all
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(ToDto);

            return new PagedResultDto<StaffDto>
            {
                Data = pageItems,
                TotalCount = all.Count,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<StaffDto?> GetByIdAsync(int id)
        {
            var staff = await _staffRepository.GetByIdAsync(id);
            return staff is null ? null : ToDto(staff);
        }

        public async Task<StaffDto?> UpdateAsync(int id, UpdateStaffDto dto)
        {
            var staff = await _staffRepository.GetByIdAsync(id);
            if (staff is null) return null;

            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                staff.Name = dto.Name;
            }
            if (!string.IsNullOrWhiteSpace(dto.Surname))
            {
                staff.Surname = dto.Surname;
            }
            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber)) {
                staff.PhoneNumber = dto.PhoneNumber;
            }
            if (!string.IsNullOrWhiteSpace(dto.JobTitle)) {
                staff.JobTitle = dto.JobTitle;
            }
            if (dto.Role is not null) {
                staff.Role = dto.Role.Value;
            }
            if (dto.IsActive is not null) {
                staff.IsActive = dto.IsActive.Value;
            }

            staff.UpdatedAt = DateTime.UtcNow;

            await _staffRepository.UpdateAsync(staff);
            return ToDto(staff);
        }

        public async Task<StaffDto?> UpdateProfilePictureAsync(int id, Stream fileStream, string fileName)
        {
            var staff = await _staffRepository.GetByIdAsync(id);
            if (staff is null)
            {
                return null;
            }

            var (url, publicId) = await _cloudinaryService.UploadImageAsync(fileStream, fileName, ProfilePictureFolder);

            var previousPublicId = staff.ProfilePicturePublicId;

            staff.ProfilePictureUrl = url;
            staff.ProfilePicturePublicId = publicId;
            staff.UpdatedAt = DateTime.UtcNow;

            await _staffRepository.UpdateAsync(staff);

            if (!string.IsNullOrWhiteSpace(previousPublicId))
            {
                await _cloudinaryService.DeleteImageAsync(previousPublicId);
            }

            return ToDto(staff);
        }

        private static string GenerateTemporaryPassword()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789!@#$%";
            var bytes = RandomNumberGenerator.GetBytes(14);
            var result = new char[14];

            for (var i = 0; i < 14; i++)
            {
                result[i] = chars[bytes[i] % chars.Length];
            }

            return new string(result);
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
