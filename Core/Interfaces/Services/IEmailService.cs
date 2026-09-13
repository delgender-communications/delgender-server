using Core.DTOs;
using Core.Entities;

namespace Core.Interfaces.Services
{
    public interface IEmailService
    {
        Task SendBookingConfirmationAsync(ConfirmationDto confirmation, string recipientEmail);

        Task SendOtpAsync(string recipientEmail, string firstName, string code, int expiryMinutes);

        Task SendBookingResponseAsync(Booking booking, string subject, string message);

        Task SendStaffWelcomeAsync(Staff staff, string temporaryPassword, string loginUrl);

        Task SendInvoiceAsync(Invoice invoice, string? message);
    }
}
