using Core.DTOs;
using Core.DTOs.Auth;

namespace Core.Interfaces.Services
{
    public interface IStaffService
    {
        Task<StaffDto> CreateAsync(CreateStaffDto dto);
        Task<PagedResultDto<StaffDto>> GetAllAsync(int page, int pageSize);
        Task<StaffDto?> GetByIdAsync(int id);
        Task<StaffDto?> UpdateAsync(int id, UpdateStaffDto dto);
        Task<StaffDto?> UpdateProfilePictureAsync(int id, Stream fileStream, string fileName);
    }
}
