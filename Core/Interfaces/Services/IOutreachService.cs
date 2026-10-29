using Core.DTOs;
using Core.DTOs.Outreach;
using Core.Enums;

namespace Core.Interfaces.Services
{
    public interface IOutreachService
    {
        Task<PagedResultDto<OutreachListItemDto>> GetAllAsync(int page, int pageSize, OutreachStatus? status);
        Task<OutreachDetailDto?> GetByIdAsync(int id);
        Task<OutreachDetailDto> CreateAsync(CreateOutreachDto dto, int staffId);
        Task<OutreachDetailDto?> UpdateAsync(int id, UpdateOutreachDto dto, int staffId);
        Task<OutreachDetailDto?> UpdateNotesAsync(int id, UpdateOutreachNotesDto dto);
        Task<OutreachDetailDto?> UploadPitchDeckAsync(int id, Stream fileStream, string fileName, int staffId);
        Task<(byte[] Bytes, string FileName)?> DownloadPitchDeckAsync(int id);
        Task<OutreachDetailDto?> UploadReportAsync(int id, Stream fileStream, string fileName, int staffId);
        Task<(byte[] Bytes, string FileName)?> DownloadReportAsync(int id);
        Task<OutreachDetailDto?> SendEmailAsync(int id, int staffId);
        Task<OutreachDetailDto?> MarkEmailedAsync(int id, int staffId);
    }
}
