using Core.DTOs;
using Core.DTOs.Booking;
using Core.Enums;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace DelgenderCommunicationsAPI.Controllers
{
    [ApiController]
    [Route("api/v1/bookings")]
    public class BookingsController : Controller
    {
        private readonly IBookingService _bookingService;

        public BookingsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpPost("create")]
        [AllowAnonymous]
        [EnableRateLimiting("booking")]
        public async Task<ActionResult<BookingDto>> Create([FromBody] CreateBookingDto dto)
        {
            var booking = await _bookingService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = booking.Id }, booking);
        }

        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<ActionResult<BookingDto>> GetById(int id)
        {
            var booking = await _bookingService.GetByIdAsync(id);
            return booking is null ? NotFound() : Ok(booking);
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<PagedResultDto<BookingDto>>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] BookingStatus? status = null)
        {
            var result = await _bookingService.GetAllAsync(page, pageSize, status);
            return Ok(result);
        }

        [HttpPatch("{id:int}/respond")]
        [Authorize]
        public async Task<ActionResult<BookingDto>> Respond(int id, [FromBody] RespondBookingDto dto)
        {
            var staffId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var booking = await _bookingService.RespondAsync(id, dto, staffId);
            return booking is null ? NotFound() : Ok(booking);
        }
    }
}
