using Core.DTOs;

namespace Core.Interfaces.Services
{
    public interface IEmailService
    {
        Task SendBookingConfirmationAsync(ConfirmationDto confirmation, string recipientEmail);
    }
}
