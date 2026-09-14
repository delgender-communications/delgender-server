using Core.DTOs;
using Core.Entities;
using Core.Enums;
using Core.Interfaces.Services;
using Resend;

namespace Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IResend _resend;
        private readonly IEmailTemplateRenderer _renderer;

        public EmailService(IResend resend, IEmailTemplateRenderer renderer)
        {
            _resend = resend;
            _renderer = renderer;
        }

        public async Task SendBookingConfirmationAsync(ConfirmationDto confirmation, string recipientEmail)
        {
            var tokens = new Dictionary<string, string?>
            {
                ["FullName"] = confirmation.FullName,
                ["BookingReference"] = confirmation.BookingReference,
                ["MeetingType"] = FormatMeetingType(confirmation.Meeting),
                ["BookingDate"] = confirmation.BookingDate.ToString("dddd, d MMMM yyyy"),
                ["BookingTime"] = confirmation.BookingTime.ToString("h:mm tt"),
                ["Year"] = DateTime.UtcNow.Year.ToString()
            };

            var message = new EmailMessage
            {
                From = "Delgender Communications <bookings@delgendercommunications.co.za>",
                To = recipientEmail,
                Subject = "Your consultation booking has been received",
                HtmlBody = _renderer.Render("BookingConfirmation.html", tokens),
                TextBody = _renderer.Render("BookingConfirmation.txt", tokens)
            };

            await _resend.EmailSendAsync(message);
        }

        public async Task SendOtpAsync(string recipientEmail, string firstName, string code, int expiryMinutes)
        {
            var tokens = new Dictionary<string, string?>
            {
                ["Heading"] = "Your sign-in code",
                ["Intro"] = $"Hi {firstName}, use this code to finish signing in to the staff portal.",
                ["Highlight"] = code,
                ["HighlightLabel"] = $"Expires in {expiryMinutes} minutes",
                ["BodyHtml"] = "",
                ["FooterNote"] = "If you didn't try to sign in, you can safely ignore this email.",
                ["Year"] = DateTime.UtcNow.Year.ToString(),
                ["FirstName"] = firstName,
                ["Code"] = code,
                ["ExpiryMinutes"] = expiryMinutes.ToString()
            };

            var message = new EmailMessage
            {
                From = "Delgender Communications <security@delgendercommunications.site>",
                To = recipientEmail,
                Subject = $"Your sign-in code is {code}",
                HtmlBody = _renderer.Render("Layout.html", tokens),
                TextBody = _renderer.Render("Otp.txt", tokens)
            };

            await _resend.EmailSendAsync(message);
        }

        public async Task SendBookingResponseAsync(Booking booking, string subject, string message)
        {
            var statusLabel = booking.Status == BookingStatus.Confirmed ? "Confirmed" : "Declined";
            var bookingDateTime = $"{booking.Date:dddd, d MMMM yyyy} at {booking.Time:h:mm tt}";

            var tokens = new Dictionary<string, string?>
            {
                ["Heading"] = subject,
                ["BodyHtml"] = FormatMessageAsHtml(message),
                ["FooterNote"] = $"Booking reference {booking.BookingReference} · {statusLabel} · {bookingDateTime}",
                ["Year"] = DateTime.UtcNow.Year.ToString(),
                ["FullName"] = booking.Customer.FullName,
                ["Message"] = message,
                ["BookingReference"] = booking.BookingReference,
                ["StatusLabel"] = statusLabel,
                ["BookingDateTime"] = bookingDateTime
            };

            var email = new EmailMessage
            {
                From = "Delgender Communications <bookings@delgendercommunications.co.za>",
                To = booking.Customer.Email,
                Subject = subject,
                HtmlBody = _renderer.Render("Layout.html", tokens),
                TextBody = _renderer.Render("BookingResponse.txt", tokens)
            };

            await _resend.EmailSendAsync(email);
        }

        public async Task SendStaffWelcomeAsync(Staff staff, string temporaryPassword, string loginUrl)
        {
            var bodyTokens = new Dictionary<string, string?>
            {
                ["StaffId"] = staff.StaffId,
                ["Email"] = staff.Email,
                ["LoginUrl"] = loginUrl
            };
            var bodyHtml = _renderer.Render("StaffWelcomeBody.html", bodyTokens);

            var tokens = new Dictionary<string, string?>
            {
                ["Heading"] = "Welcome to the team",
                ["Intro"] = $"Hi {staff.Name}, an account has been created for you on the Delgender Communications staff portal.",
                ["Highlight"] = temporaryPassword,
                ["HighlightLabel"] = "Temporary password",
                ["BodyHtml"] = bodyHtml,
                ["FooterNote"] = "For security, please change your password as soon as you sign in.",
                ["Year"] = DateTime.UtcNow.Year.ToString(),
                ["Name"] = staff.Name,
                ["StaffId"] = staff.StaffId,
                ["Email"] = staff.Email,
                ["TemporaryPassword"] = temporaryPassword,
                ["LoginUrl"] = loginUrl
            };

            var message = new EmailMessage
            {
                From = "Delgender Communications <security@delgendercommunications.site>",
                To = staff.Email,
                Subject = "Your Delgender Communications staff account",
                HtmlBody = _renderer.Render("Layout.html", tokens),
                TextBody = _renderer.Render("StaffWelcome.txt", tokens)
            };

            await _resend.EmailSendAsync(message);
        }

        public async Task SendInvoiceAsync(Invoice invoice, string? message, byte[] pdfBytes)
        {
            var fileName = $"{invoice.InvoiceNumber}.pdf";
            var dueDate = invoice.DueDate.ToString("d MMMM yyyy");
            var totalDue = $"R{invoice.TotalAmount:N2}";

            var bodyTokens = new Dictionary<string, string?>
            {
                ["MessageHtml"] = string.IsNullOrWhiteSpace(message) ? null : FormatMessageAsHtml(message),
                ["InvoiceNumber"] = invoice.InvoiceNumber,
                ["DueDate"] = dueDate,
                ["TotalDue"] = totalDue,
                ["InvoiceFileName"] = fileName
            };
            var bodyHtml = _renderer.Render("InvoiceBody.html", bodyTokens);

            var tokens = new Dictionary<string, string?>
            {
                ["Heading"] = $"Invoice {invoice.InvoiceNumber}",
                ["Intro"] = $"Hi {invoice.Customer.FullName},",
                ["BodyHtml"] = bodyHtml,
                ["FooterNote"] = "Please do not reply to this email. This mailbox is not monitored.",
                ["Year"] = DateTime.UtcNow.Year.ToString(),
                ["FullName"] = invoice.Customer.FullName,
                ["Message"] = message,
                ["InvoiceNumber"] = invoice.InvoiceNumber,
                ["DueDate"] = dueDate,
                ["TotalDue"] = totalDue
            };

            var email = new EmailMessage
            {
                From = "Delgender Communications <billing@delgendercommunications.co.za>",
                To = invoice.Customer.Email,
                Subject = $"Invoice {invoice.InvoiceNumber} from Delgender Communications",
                HtmlBody = _renderer.Render("Layout.html", tokens),
                TextBody = _renderer.Render("Invoice.txt", tokens),
                Attachments = new List<EmailAttachment>
                {
                    new EmailAttachment
                    {
                        Filename = fileName,
                        Content = pdfBytes
                    }
                }
            };

            await _resend.EmailSendAsync(email);
        }

        private static string FormatMessageAsHtml(string message) =>
            string.Join("", message
                .Split('\n')
                .Select(line => $"<p style=\"margin:0 0 14px;font-size:15px;line-height:24px;color:#374151;\">{System.Net.WebUtility.HtmlEncode(line)}</p>"));

        private static string FormatMeetingType(MeetingType meeting) => meeting switch
        {
            MeetingType.InPerson => "In-person",
            MeetingType.OnlineMeeting => "Online Meeting",
            MeetingType.PhoneCall => "Phone Call",
            _ => meeting.ToString()
        };
    }
}
