using Core.DTOs;
using Core.Entities;
using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;

namespace Application.Services
{
    public class BookingService : IBookingService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IConfirmationRepository _confirmationRepository;
        private readonly IEmailService _emailService;

        public BookingService(IBookingRepository bookingRepository, IConfirmationRepository confirmationRepository,
            IEmailService emailService, ICustomerRepository customerRepository)
        {
            _bookingRepository = bookingRepository;
            _confirmationRepository = confirmationRepository;
            _emailService = emailService;
            _customerRepository = customerRepository;
        }

        public async Task<BookingDto> CreateAsync(CreateBookingDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto), "CreateBookingDto cannot be null.");
            }

            var customer = new Customer
            {
                FullName = dto.FullName,
                JobTitle = dto.JobTitle,
                CompanyName = dto.CompanyName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Industry = dto.Industry,
                ContactPermission = dto.ContactPermission
            };

            customer = await _customerRepository.CreateAsync(customer);

            var booking = new Booking
            {
                CustomerId = customer.Id,
                HelpWith = dto.HelpWith,
                ProblemDescription = dto.ProblemDescription,
                SessionGoal = dto.SessionGoal,
                Meeting = dto.Meeting,
                Date = dto.Date,
                Time = dto.Time,
            };

            booking = await _bookingRepository.CreateAsync(booking);

            var confirmation = new Confirmation
            {
                BookingId = booking.Id,
                Status = ConfirmationStatus.Pending,
            };

            confirmation = await _confirmationRepository.CreateAsync(confirmation);

            await SendConfirmationAsync(booking, confirmation);

            return ToDto(booking);
        }

        public async Task<BookingDto?> GetByIdAsync(int id)
        {
            var booking = await _bookingRepository.GetByIdBookingAsync(id);
            return booking is null ? null : ToDto(booking);
        }

        public async Task<PagedResultDto<BookingDto>> GetAllAsync(int page, int pageSize)
        {
            var bookings = await _bookingRepository.GetAllBookingsAsync(page, pageSize);
            var totalCount = await _bookingRepository.GetTotalCountAsync();

            return new PagedResultDto<BookingDto>
            {
                Data = bookings.Select(ToDto),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        private async Task SendConfirmationAsync(Booking booking, Confirmation confirmation)
        {
            var confirmationDto = new ConfirmationDto
            {
                Id = confirmation.Id,
                BookingId = booking.Id,
                FullName = booking.Customer.FullName,
                CompanyName = booking.Customer.CompanyName,
                HelpWith = booking.HelpWith,
                Meeting = booking.Meeting,
                BookingDate = booking.Date,
                BookingTime = booking.Time
            };

            try
            {
                await _emailService.SendBookingConfirmationAsync(confirmationDto, booking.Customer.Email);
                confirmation.Status = ConfirmationStatus.Sent;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send confirmation email: {ex.Message}");
                confirmation.Status = ConfirmationStatus.Failed;
                confirmation.FailureReason = ex.Message;
            }

            await _confirmationRepository.UpdateAsync(confirmation);
        }

        private static BookingDto ToDto(Booking booking) => new()
        {
            Id = booking.Id,
            FullName = booking.Customer.FullName,
            JobTitle = booking.Customer.JobTitle,
            CompanyName = booking.Customer.CompanyName,
            Email = booking.Customer.Email,
            PhoneNumber = booking.Customer.PhoneNumber,
            Industry = booking.Customer.Industry,
            HelpWith = booking.HelpWith,
            ProblemDescription = booking.ProblemDescription,
            SessionGoal = booking.SessionGoal,
            Meeting = booking.Meeting,
            Date = booking.Date,
            Time = booking.Time,
            ContactPermission = booking.Customer.ContactPermission,
            CreatedAt = booking.CreatedAt,
            UpdatedAt = booking.UpdatedAt
        };
    }
}
