using Core.DTOs;
using Core.DTOs.Booking;
using Core.Enums;

namespace Core.Interfaces.Services
{
    public interface IBookingService
    {
        Task<BookingDto> CreateAsync(CreateBookingDto dto);
        Task<BookingDto?> GetByIdAsync(int id);
        Task<PagedResultDto<BookingDto>> GetAllAsync(int page, int pageSize, BookingStatus? status);
        Task<BookingDto?> RespondAsync(int id, RespondBookingDto dto, int staffId);
    }
}
