using Core.DTOs;
using Core.DTOs.Booking;
using Core.Entities;
using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Services
{
    public class BookingService : IBookingService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IConfirmationRepository _confirmationRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IBackgroundTaskQueue _backgroundTaskQueue;

        public BookingService(IBookingRepository bookingRepository, IConfirmationRepository confirmationRepository,
            ICustomerRepository customerRepository, IStaffRepository staffRepository, IBackgroundTaskQueue backgroundTaskQueue)
        {
            _bookingRepository = bookingRepository;
            _confirmationRepository = confirmationRepository;
            _customerRepository = customerRepository;
            _staffRepository = staffRepository;
            _backgroundTaskQueue = backgroundTaskQueue;
        }

        public async Task<BookingDto> CreateAsync(CreateBookingDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(
                    nameof(dto),
                    "CreateBookingDto cannot be null.");
            }

            var customer = new Customer
            {
                FullName = dto.FullName,
                JobTitle = dto.JobTitle,
                CompanyName = dto.CompanyName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Industry = dto.Industry,
                IdNumber = dto.IdNumber,
                ContactPermission = dto.ContactPermission
            };

            customer = await _customerRepository.CreateAsync(customer);

            var referenceNumber =
                await _bookingRepository.GetNextBookingReferenceNumberAsync();

            var booking = new Booking
            {
                CustomerId = customer.Id,
                BookingReference = $"#BK-{referenceNumber:D4}",
                HelpWith = dto.HelpWith,
                ProblemDescription = dto.ProblemDescription,
                SessionGoal = dto.SessionGoal,
                Meeting = dto.Meeting,
                Date = dto.Date,
                Time = dto.Time,
                Status = BookingStatus.Pending
            };

            booking = await _bookingRepository.CreateAsync(booking);

            var confirmation = new Confirmation
            {
                BookingId = booking.Id,
                Status = ConfirmationStatus.Pending,
            };

            confirmation = await _confirmationRepository.CreateAsync(confirmation);

            await SendConfirmationAsync(booking, confirmation);

            booking.Customer = customer;

            return ToDto(booking);
        }

        public async Task<BookingDto?> GetByIdAsync(int id)
        {
            var booking = await _bookingRepository.GetByIdBookingAsync(id);
            return booking is null ? null : ToDto(booking);
        }

        public async Task<PagedResultDto<BookingDto>> GetAllAsync(int page, int pageSize, BookingStatus? status)
        {
            var bookings = await _bookingRepository.GetAllBookingsAsync(page, pageSize, status);
            var totalCount = await _bookingRepository.GetTotalCountAsync(status);

            return new PagedResultDto<BookingDto>
            {
                Data = bookings.Select(ToDto),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<BookingDto?> RespondAsync(int id, RespondBookingDto dto, int staffId)
        {
            if (dto.Status is not (BookingStatus.Confirmed or BookingStatus.Declined))
            {
                throw new ArgumentException(
                    "A response must either confirm or decline the booking.");
            }

            var booking = await _bookingRepository.GetByIdBookingAsync(id)
                ?? throw new KeyNotFoundException("Booking not found.");

            if (booking.Status != BookingStatus.Pending)
            {
                throw new InvalidOperationException(
                    "This booking has already been responded to.");
            }

            var staff = await _staffRepository.GetByIdAsync(staffId)
                ?? throw new KeyNotFoundException("Staff member not found.");

            booking.Status = dto.Status;
            booking.RespondedByStaffId = staffId;
            booking.RespondedAt = DateTime.UtcNow;
            booking.ResponseMessage = dto.Message;
            booking.DeclineReason =
                dto.Status == BookingStatus.Declined
                    ? dto.DeclineReason
                    : null;
            booking.UpdatedAt = DateTime.UtcNow;
            booking.RespondedByStaff = staff;

            await _bookingRepository.UpdateAsync(booking);

            var subject = dto.Status == BookingStatus.Confirmed
                ? "Your consultation booking has been confirmed"
                : "An update on your consultation booking request";

            _backgroundTaskQueue.Enqueue(async (services, cancellationToken) =>
            {
                var bookingRepository =
                    services.GetRequiredService<IBookingRepository>();

                var emailService =
                    services.GetRequiredService<IEmailService>();

                var booking = await bookingRepository.GetByIdBookingAsync(id);

                if (booking is null)
                {
                    return;
                }

                await emailService.SendBookingResponseAsync(
                    booking,
                    subject,
                    dto.Message);
            });

            return ToDto(booking);
        }

        private async Task SendConfirmationAsync(Booking booking, Confirmation confirmation)
        {
            var bookingId = booking.Id;
            var confirmationId = confirmation.Id;

            _backgroundTaskQueue.Enqueue(async (services, cancellationToken) =>
            {
                var confirmationRepository =
                    services.GetRequiredService<IConfirmationRepository>();

                var bookingRepository =
                    services.GetRequiredService<IBookingRepository>();

                var emailService =
                    services.GetRequiredService<IEmailService>();

                Confirmation? currentConfirmation = null;

                try
                {
                    var currentBooking =
                        await bookingRepository.GetByIdBookingAsync(bookingId);

                    if (currentBooking is null)
                    {
                        return;
                    }

                    currentConfirmation =
                        await confirmationRepository.GetByIdAsync(confirmationId);

                    if (currentConfirmation is null)
                    {
                        return;
                    }

                    var confirmationDto = new ConfirmationDto
                    {
                        Id = currentConfirmation.Id,
                        BookingId = currentBooking.Id,
                        FullName = currentBooking.Customer.FullName,
                        CompanyName = currentBooking.Customer.CompanyName,
                        HelpWith = currentBooking.HelpWith,
                        Meeting = currentBooking.Meeting,
                        BookingDate = currentBooking.Date,
                        BookingTime = currentBooking.Time
                    };

                    await emailService.SendBookingConfirmationAsync(
                        confirmationDto,
                        currentBooking.Customer.Email);

                    currentConfirmation.Status = ConfirmationStatus.Sent;
                    currentConfirmation.FailureReason = null;

                    await confirmationRepository.UpdateAsync(
                        currentConfirmation);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"Failed to send confirmation email: {ex}");

                    if (currentConfirmation is not null)
                    {
                        currentConfirmation.Status = ConfirmationStatus.Failed;
                        currentConfirmation.FailureReason = ex.Message;

                        await confirmationRepository.UpdateAsync(
                            currentConfirmation);
                    }
                }
            });
        }

        private static BookingDto ToDto(Booking booking) => new()
        {
            Id = booking.Id,
            BookingReference = booking.BookingReference,

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
            UpdatedAt = booking.UpdatedAt,

            Status = booking.Status,

            RespondedByStaffName = booking.RespondedByStaff is null
        ? null
        : $"{booking.RespondedByStaff.Name} {booking.RespondedByStaff.Surname}",

            RespondedAt = booking.RespondedAt,
            ResponseMessage = booking.ResponseMessage,
            DeclineReason = booking.DeclineReason
        };
    }
}
