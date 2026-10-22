using Core.DTOs;
using Core.DTOs.ResearchLeads;
using Core.Enums;

namespace Core.Interfaces.Services
{
    public interface IResearchLeadService
    {
        Task<IngestResultDto> IngestAsync(IngestResearchLeadsDto dto);
        Task<ResearchLeadDto> AddManualAsync(ResearchLeadInputDto dto, int staffId);
        Task<PagedResultDto<ResearchLeadDto>> GetAllAsync(int page, int pageSize, ResearchLeadStatus? status, ResearchLeadCase? leadCase);
        Task<int> GetNewCountAsync();
        Task<ResearchLeadDto?> GetByIdAsync(int id);
        Task<ResearchLeadDto?> AcceptAsync(int id, int staffId);
        Task<ResearchLeadDto?> DismissAsync(int id, DismissResearchLeadDto dto, int staffId);
    }
}
