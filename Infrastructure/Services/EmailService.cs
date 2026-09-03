using Core.Configuration;
using Core.DTOs;
using Core.Interfaces.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task SendBookingConfirmationAsync(ConfirmationDto confirmation, string recipientEmail)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            message.To.Add(MailboxAddress.Parse(recipientEmail));
            message.Subject = "Your consultation booking is received";

            message.Body = new TextPart("plain")
            {
                Text = BuildBody(confirmation)
            };

            using var client = new SmtpClient();

            try
            {
                await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_settings.SenderEmail, _settings.SenderPassword);
                await client.SendAsync(message);
            }
            finally
            {
                if (client.IsConnected)
                    await client.DisconnectAsync(true);
            }
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
