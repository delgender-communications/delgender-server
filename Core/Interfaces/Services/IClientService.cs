using Core.DTOs;
using Core.DTOs.Client;
using Core.Enums;

namespace Core.Interfaces.Services
{
    public interface IClientService
    {
        Task<PagedResultDto<ClientDto>> GetAllAsync(int page, int pageSize, ClientStatus? status);
        Task<ClientDto> CreateAsync(CreateClientDto dto);
        Task<ClientDto?> UpdateStatusAsync(int id, UpdateClientStatusDto dto, int staffId);
    }
}
