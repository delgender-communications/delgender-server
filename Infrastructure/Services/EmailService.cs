using Core.DTOs;
using Core.Interfaces.Services;
using Resend;

namespace Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IResend _resend;

        public EmailService(IResend resend)
        {
            _resend = resend;
        }

        public async Task SendBookingConfirmationAsync(
            ConfirmationDto confirmation,
            string recipientEmail)
        {
            var message = new EmailMessage
            {
                From = "Delgender Communications <bookings@delgendercommunications.site>",
                To = recipientEmail,
                Subject = "Your consultation booking is received",
                TextBody = BuildBody(confirmation)
            };

            await _resend.EmailSendAsync(message);
        }

        private static string BuildBody(ConfirmationDto confirmation)
        {
            return $"""
                Hi {confirmation.FullName},

                We've received your consultation booking request.

                Meeting type: {confirmation.Meeting}
                Date: {confirmation.BookingDate:dddd, d MMMM yyyy}
                Time: {confirmation.BookingTime:h:mm tt}

                We'll confirm your appointment within 24 hours.
                """;
        }
    }
}
