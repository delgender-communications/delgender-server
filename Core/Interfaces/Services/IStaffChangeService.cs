using Core.DTOs.StaffRequests;
using Core.Enums;

namespace Core.Interfaces.Services
{
    public interface IStaffChangeService
    {
        Task<StaffChangeRequestDto> CreateAsync(int staffId, CreateStaffChangeRequestDto dto);
        Task<List<StaffChangeRequestDto>> GetMineAsync(int staffId);
        Task<bool> CancelAsync(int staffId, int requestId);

        // Admin only (the controller enforces it).
        Task<List<StaffChangeRequestDto>> GetByStatusAsync(ChangeRequestStatus? status);
        Task<StaffChangeRequestDto> ApproveAsync(int adminId, int requestId, string? note);
        Task<StaffChangeRequestDto> RejectAsync(int adminId, int requestId, string? note);
    }
}
