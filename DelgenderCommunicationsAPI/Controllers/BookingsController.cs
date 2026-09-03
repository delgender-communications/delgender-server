using Core.DTOs;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DelgenderCommunicationsAPI.Controllers
{
    [ApiController]
    [Route("api/bookings")]
    public class BookingsController : Controller
    {
        private readonly IBookingService _bookingService;

        public BookingsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpPost("create")]
        [EnableRateLimiting("booking")]
        public async Task<ActionResult<BookingDto>> Create([FromBody] CreateBookingDto dto)
        {
            var booking = await _bookingService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = booking.Id }, booking);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<BookingDto>> GetById(int id)
        {
            var booking = await _bookingService.GetByIdAsync(id);
            return booking is null ? NotFound() : Ok(booking);
        }

        [HttpGet]
        public async Task<ActionResult<PagedResultDto<BookingDto>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _bookingService.GetAllAsync(page, pageSize);
            return Ok(result);
        }
    }
}
