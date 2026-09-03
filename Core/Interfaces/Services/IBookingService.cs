using Core.DTOs;

namespace Core.Interfaces.Services
{
    public interface IBookingService
    {
        Task<BookingDto> CreateAsync(CreateBookingDto dto);
        Task<BookingDto?> GetByIdAsync(int id);
        Task<PagedResultDto<BookingDto>> GetAllAsync(int page, int pageSize);
    }
}
