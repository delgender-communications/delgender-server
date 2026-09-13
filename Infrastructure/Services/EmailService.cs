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

        public EmailService(IResend resend)
        {
            _resend = resend;
        }

        public async Task SendOtpAsync(string recipientEmail, string firstName, string code, int expiryMinutes)
        {
            var message = new EmailMessage
            {
                From = "Delgender Communications <staff@delgendercommunications.site>",
                To = recipientEmail,
                Subject = $"Your sign-in code is {code}",
                HtmlBody = BuildSimpleHtml(
                    heading: "Your sign-in code",
                    intro: $"Hi {firstName}, use this code to finish signing in to the staff portal.",
                    highlight: code,
                    highlightLabel: $"Expires in {expiryMinutes} minutes",
                    footerNote: "If you didn't try to sign in, you can safely ignore this email."),
                TextBody = $"Hi {firstName},\n\nYour sign-in code is {code}. It expires in {expiryMinutes} minutes.\n\nIf you didn't try to sign in, you can safely ignore this email.\n\n© {DateTime.UtcNow.Year} Delgender Communications"
            };

            await _resend.EmailSendAsync(message);
        }

        public async Task SendBookingResponseAsync(Booking booking, string subject, string message)
        {
            var statusLabel = booking.Status == BookingStatus.Confirmed ? "Confirmed" : "Declined";

            var email = new EmailMessage
            {
                From = "Delgender Communications <bookings@delgendercommunications.site>",
                To = booking.Customer.Email,
                Subject = subject,
                HtmlBody = BuildSimpleHtml(
                    heading: subject,
                    intro: $"Hi {booking.Customer.FullName},",
                    highlight: null,
                    highlightLabel: null,
                    bodyHtml: FormatMessageAsHtml(message),
                    footerNote: $"Booking reference #{booking.Id} · {statusLabel} · {booking.Date:dddd, d MMMM yyyy} at {booking.Time:h:mm tt}"),
                TextBody = $"Hi {booking.Customer.FullName},\n\n{message}\n\nBooking reference #{booking.Id} · {statusLabel} · {booking.Date:dddd, d MMMM yyyy} at {booking.Time:h:mm tt}\n\n© {DateTime.UtcNow.Year} Delgender Communications"
            };

            await _resend.EmailSendAsync(email);
        }

        public async Task SendStaffWelcomeAsync(Staff staff, string temporaryPassword, string loginUrl)
        {
            var message = new EmailMessage
            {
                From = "Delgender Communications <staff@delgendercommunications.site>",
                To = staff.Email,
                Subject = "Your Delgender Communications staff account",
                HtmlBody = BuildSimpleHtml(
                    heading: "Welcome to the team",
                    intro: $"Hi {staff.Name}, an account has been created for you on the Delgender Communications staff portal.",
                    highlight: temporaryPassword,
                    highlightLabel: "Temporary password",
                    bodyHtml: $"""
                        <p style="margin:0 0 6px;font-size:14px;color:#4b5563;">Staff ID: <strong>{staff.StaffId}</strong></p>
                        <p style="margin:0 0 20px;font-size:14px;color:#4b5563;">Email: <strong>{staff.Email}</strong></p>
                        <p style="margin:0 0 20px;font-size:15px;color:#4b5563;">
                            Sign in and you'll be asked to set a new password and, if enabled, verify a one-time
                            code sent to this inbox.
                        </p>
                        <table role="presentation" cellspacing="0" cellpadding="0" border="0" style="margin:0 auto;">
                            <tr><td align="center" style="border-radius:8px;background-color:#12d3de;">
                                <a href="{loginUrl}" style="display:inline-block;padding:12px 26px;font-size:14px;font-weight:bold;color:#00191b;text-decoration:none;">
                                    Go to staff portal
                                </a>
                            </td></tr>
                        </table>
                        """,
                    footerNote: "For security, please change your password as soon as you sign in."),
                TextBody = $"Hi {staff.Name},\n\nAn account has been created for you on the Delgender Communications staff portal.\n\nStaff ID: {staff.StaffId}\nEmail: {staff.Email}\nTemporary password: {temporaryPassword}\n\nSign in at {loginUrl} and set a new password.\n\n© {DateTime.UtcNow.Year} Delgender Communications"
            };

            await _resend.EmailSendAsync(message);
        }

        public async Task SendInvoiceAsync(Invoice invoice, string? message)
        {
            var rows = string.Join("", invoice.Items.Select(i => $"""
                <tr>
                    <td style="padding:8px 0;font-size:14px;color:#374151;border-top:1px solid #e5e7eb;">{i.Description}</td>
                    <td align="right" style="padding:8px 0;font-size:14px;color:#374151;border-top:1px solid #e5e7eb;">{i.Quantity}</td>
                    <td align="right" style="padding:8px 0;font-size:14px;color:#374151;border-top:1px solid #e5e7eb;">R{i.UnitPrice:N2}</td>
                    <td align="right" style="padding:8px 0;font-size:14px;font-weight:bold;color:#111827;border-top:1px solid #e5e7eb;">R{i.TotalAmount:N2}</td>
                </tr>
                """));

            var bodyHtml = $"""
                {(string.IsNullOrWhiteSpace(message) ? "" : $"<p style=\"margin:0 0 20px;font-size:15px;color:#4b5563;\">{FormatMessageAsHtml(message)}</p>")}
                <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="margin-bottom:16px;">
                    <tr>
                        <td style="padding:6px 0;font-size:14px;color:#6b7280;">Invoice</td>
                        <td align="right" style="padding:6px 0;font-size:14px;font-weight:bold;color:#111827;">{invoice.InvoiceNumber}</td>
                    </tr>
                    <tr>
                        <td style="padding:6px 0;font-size:14px;color:#6b7280;">Due date</td>
                        <td align="right" style="padding:6px 0;font-size:14px;font-weight:bold;color:#111827;">{invoice.DueDate:d MMMM yyyy}</td>
                    </tr>
                </table>
                <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0">
                    <tr>
                        <td style="padding-bottom:8px;font-size:12px;font-weight:bold;text-transform:uppercase;color:#6b7280;">Description</td>
                        <td align="right" style="padding-bottom:8px;font-size:12px;font-weight:bold;text-transform:uppercase;color:#6b7280;">Qty</td>
                        <td align="right" style="padding-bottom:8px;font-size:12px;font-weight:bold;text-transform:uppercase;color:#6b7280;">Rate</td>
                        <td align="right" style="padding-bottom:8px;font-size:12px;font-weight:bold;text-transform:uppercase;color:#6b7280;">Amount</td>
                    </tr>
                    {rows}
                </table>
                <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="margin-top:16px;">
                    <tr><td align="right" style="padding:4px 0;font-size:20px;font-weight:bold;color:#111827;">Total due: R{invoice.TotalAmount:N2}</td></tr>
                </table>
                """;

            var email = new EmailMessage
            {
                From = "Delgender Communications <billing@delgendercommunications.site>",
                To = invoice.Customer.Email,
                Subject = $"Invoice {invoice.InvoiceNumber} from Delgender Communications",
                HtmlBody = BuildSimpleHtml(
                    heading: $"Invoice {invoice.InvoiceNumber}",
                    intro: $"Hi {invoice.Customer.FullName},",
                    highlight: null,
                    highlightLabel: null,
                    bodyHtml: bodyHtml,
                    footerNote: "Please do not reply to this email. This mailbox is not monitored."),
                TextBody = $"Hi {invoice.Customer.FullName},\n\nInvoice {invoice.InvoiceNumber} — total due R{invoice.TotalAmount:N2}, due {invoice.DueDate:d MMMM yyyy}.\n\n© {DateTime.UtcNow.Year} Delgender Communications"
            };

            await _resend.EmailSendAsync(email);
        }

        private static string FormatMessageAsHtml(string message) =>
            string.Join("", message
                .Split('\n')
                .Select(line => $"<p style=\"margin:0 0 14px;font-size:15px;line-height:24px;color:#374151;\">{System.Net.WebUtility.HtmlEncode(line)}</p>"));

        private static string BuildSimpleHtml(
            string heading,
            string intro,
            string? highlight,
            string? highlightLabel,
            string? footerNote,
            string? bodyHtml = null)
        {
            var highlightBlock = highlight is null ? "" : $"""
                <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="margin:0 0 24px;">
                    <tr><td align="center" style="padding:22px 24px;background-color:#f8fafc;border:1px solid #e5e7eb;border-radius:10px;">
                        <p style="margin:0 0 6px;font-size:32px;font-weight:bold;letter-spacing:6px;color:#111827;">{highlight}</p>
                        <p style="margin:0;font-size:12px;color:#6b7280;">{highlightLabel}</p>
                    </td></tr>
                </table>
                """;

            return $"""
                <!DOCTYPE html>
                <html lang="en">
                <head><meta charset="UTF-8"><meta name="viewport" content="width=device-width, initial-scale=1.0"><title>{heading}</title></head>
                <body style="margin:0;padding:0;background-color:#f4f6f8;font-family:Arial, Helvetica, sans-serif;color:#1f2937;">
                    <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="background-color:#f4f6f8;padding:40px 16px;">
                        <tr><td align="center">
                            <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="max-width:600px;background-color:#ffffff;border-radius:12px;overflow:hidden;">
                                <tr><td align="center" style="padding:32px 30px 24px;">
                                    <img src="https://delgendercommunications.site/favicon.png" alt="Delgender Communications" width="160" style="display:block;max-width:160px;height:auto;margin-bottom:24px;">
                                    <h1 style="margin:0;font-size:24px;line-height:32px;color:#111827;">{heading}</h1>
                                </td></tr>
                                <tr><td style="padding:0 30px 32px;">
                                    <p style="margin:0 0 20px;font-size:16px;line-height:26px;">{intro}</p>
                                    {highlightBlock}
                                    {bodyHtml ?? ""}
                                </td></tr>
                                <tr><td style="padding:24px 30px;background-color:#f8fafc;border-top:1px solid #e5e7eb;text-align:center;">
                                    <p style="margin:0 0 8px;font-size:12px;line-height:19px;color:#6b7280;">{footerNote}</p>
                                    <p style="margin:0;font-size:11px;color:#9ca3af;">© {DateTime.UtcNow.Year} Delgender Communications</p>
                                </td></tr>
                            </table>
                        </td></tr>
                    </table>
                </body>
                </html>
                """;
        }

        public async Task SendBookingConfirmationAsync(
            ConfirmationDto confirmation,
            string recipientEmail)
        {
            var message = new EmailMessage
            {
                From = "Delgender Communications <bookings@delgendercommunications.site>",
                To = recipientEmail,
                Subject = "Your consultation booking has been received",
                HtmlBody = BuildHtmlBody(confirmation),
                TextBody = BuildTextBody(confirmation)
            };

            await _resend.EmailSendAsync(message);
        }

        private static string BuildHtmlBody(ConfirmationDto confirmation)
        {
            return $"""
                <!DOCTYPE html>
                <html lang="en">
                <head>
                    <meta charset="UTF-8">
                    <meta name="viewport" content="width=device-width, initial-scale=1.0">
                    <title>Booking Confirmation</title>
                </head>

                <body style="
                    margin: 0;
                    padding: 0;
                    background-color: #f4f6f8;
                    font-family: Arial, Helvetica, sans-serif;
                    color: #1f2937;
                ">

                    <table
                        role="presentation"
                        width="100%"
                        cellspacing="0"
                        cellpadding="0"
                        border="0"
                        style="background-color: #f4f6f8; padding: 40px 16px;"
                    >
                        <tr>
                            <td align="center">

                                <table
                                    role="presentation"
                                    width="100%"
                                    cellspacing="0"
                                    cellpadding="0"
                                    border="0"
                                    style="
                                        max-width: 600px;
                                        background-color: #ffffff;
                                        border-radius: 12px;
                                        overflow: hidden;
                                    "
                                >

                                    <!-- Header -->
                                    <tr>
                                        <td align="center" style="padding: 32px 30px 24px;">

                                            <img
                                                src="https://delgendercommunications.site/favicon.png"
                                                alt="Delgender Communications"
                                                width="180"
                                                style="
                                                    display: block;
                                                    max-width: 180px;
                                                    height: auto;
                                                    margin-bottom: 24px;
                                                "
                                            >

                                            <h1 style="
                                                margin: 0;
                                                font-size: 26px;
                                                line-height: 34px;
                                                color: #111827;
                                            ">
                                                Booking request received
                                            </h1>

                                            <p style="
                                                margin: 10px 0 0;
                                                font-size: 15px;
                                                line-height: 24px;
                                                color: #6b7280;
                                            ">
                                                We've received your consultation booking request.
                                            </p>

                                        </td>
                                    </tr>

                                    <!-- Main content -->
                                    <tr>
                                        <td style="padding: 0 30px 32px;">

                                            <p style="
                                                margin: 0 0 20px;
                                                font-size: 16px;
                                                line-height: 26px;
                                            ">
                                                Hi <strong>{confirmation.FullName}</strong>,
                                            </p>

                                            <p style="
                                                margin: 0 0 28px;
                                                font-size: 15px;
                                                line-height: 25px;
                                                color: #4b5563;
                                            ">
                                                Thank you for choosing Delgender Communications.
                                                Your consultation request has been successfully
                                                received and is currently being reviewed.
                                            </p>

                                            <!-- Booking card -->
                                            <table
                                                role="presentation"
                                                width="100%"
                                                cellspacing="0"
                                                cellpadding="0"
                                                border="0"
                                                style="
                                                    background-color: #f8fafc;
                                                    border: 1px solid #e5e7eb;
                                                    border-radius: 10px;
                                                "
                                            >
                                                <tr>
                                                    <td style="padding: 22px 24px;">

                                                        <p style="
                                                            margin: 0 0 18px;
                                                            font-size: 13px;
                                                            font-weight: bold;
                                                            text-transform: uppercase;
                                                            letter-spacing: 0.8px;
                                                            color: #6b7280;
                                                        ">
                                                            Booking details
                                                        </p>

                                                        <table
                                                            role="presentation"
                                                            width="100%"
                                                            cellspacing="0"
                                                            cellpadding="0"
                                                            border="0"
                                                        >

                                                            <tr>
                                                                <td style="
                                                                    padding: 8px 0;
                                                                    font-size: 14px;
                                                                    color: #6b7280;
                                                                ">
                                                                    Meeting type
                                                                </td>

                                                                <td align="right" style="
                                                                    padding: 8px 0;
                                                                    font-size: 14px;
                                                                    font-weight: bold;
                                                                    color: #111827;
                                                                ">
                                                                    {FormatMeetingType(confirmation.Meeting)}
                                                                </td>
                                                            </tr>

                                                            <tr>
                                                                <td style="
                                                                    padding: 8px 0;
                                                                    font-size: 14px;
                                                                    color: #6b7280;
                                                                ">
                                                                    Date
                                                                </td>

                                                                <td align="right" style="
                                                                    padding: 8px 0;
                                                                    font-size: 14px;
                                                                    font-weight: bold;
                                                                    color: #111827;
                                                                ">
                                                                    {confirmation.BookingDate:dddd, d MMMM yyyy}
                                                                </td>
                                                            </tr>

                                                            <tr>
                                                                <td style="
                                                                    padding: 8px 0;
                                                                    font-size: 14px;
                                                                    color: #6b7280;
                                                                ">
                                                                    Time
                                                                </td>

                                                                <td align="right" style="
                                                                    padding: 8px 0;
                                                                    font-size: 14px;
                                                                    font-weight: bold;
                                                                    color: #111827;
                                                                ">
                                                                    {confirmation.BookingTime:h:mm tt}
                                                                </td>
                                                            </tr>

                                                        </table>

                                                    </td>
                                                </tr>
                                            </table>

                                            <!-- What's next -->
                                            <table
                                                role="presentation"
                                                width="100%"
                                                cellspacing="0"
                                                cellpadding="0"
                                                border="0"
                                                style="margin-top: 24px;"
                                            >
                                                <tr>
                                                    <td style="
                                                        padding: 20px;
                                                        background-color: #f0fdf4;
                                                        border-radius: 10px;
                                                    ">

                                                        <p style="
                                                            margin: 0 0 6px;
                                                            font-size: 15px;
                                                            font-weight: bold;
                                                            color: #166534;
                                                        ">
                                                            What's next?
                                                        </p>

                                                        <p style="
                                                            margin: 0;
                                                            font-size: 14px;
                                                            line-height: 23px;
                                                            color: #166534;
                                                        ">
                                                            We'll review your request and confirm your
                                                            appointment within 24 hours.
                                                        </p>

                                                    </td>
                                                </tr>
                                            </table>

                                        </td>
                                    </tr>

                                    <!-- Footer -->
                                    <tr>
                                        <td style="
                                            padding: 24px 30px;
                                            background-color: #f8fafc;
                                            border-top: 1px solid #e5e7eb;
                                            text-align: center;
                                        ">

                                            <p style="
                                                margin: 0 0 8px;
                                                font-size: 12px;
                                                line-height: 19px;
                                                color: #6b7280;
                                            ">
                                                Please do not reply to this email.
                                                This mailbox is not monitored.
                                            </p>

                                            <p style="
                                                margin: 0 0 14px;
                                                font-size: 12px;
                                                line-height: 19px;
                                                color: #6b7280;
                                            ">
                                                If you need to contact us regarding your booking,
                                                please email
                                                <a
                                                    href="mailto:delgendercommunications@gmail.com"
                                                    style="
                                                        color: #111827;
                                                        font-weight: bold;
                                                        text-decoration: none;
                                                    "
                                                >
                                                    delgendercommunications@gmail.com
                                                </a>
                                            </p>

                                            <p style="
                                                margin: 0;
                                                font-size: 11px;
                                                color: #9ca3af;
                                            ">
                                                © {DateTime.UtcNow.Year} Delgender Communications
                                            </p>

                                        </td>
                                    </tr>

                                </table>

                            </td>
                        </tr>
                    </table>

                </body>
                </html>
                """;
        }

        private static string BuildTextBody(ConfirmationDto confirmation)
        {
            return $"""
                Hi {confirmation.FullName},

                We've received your consultation booking request.

                BOOKING DETAILS
                ----------------
                Meeting type: {FormatMeetingType(confirmation.Meeting)}
                Date: {confirmation.BookingDate:dddd, d MMMM yyyy}
                Time: {confirmation.BookingTime:h:mm tt}

                WHAT'S NEXT?
                ----------------
                We'll review your request and confirm your appointment within 24 hours.

                Please do not reply to this email.
                This mailbox is not monitored.

                If you need to contact us regarding your booking,
                please email delgendercommunications@gmail.com.

                © {DateTime.UtcNow.Year} Delgender Communications
                """;
        }

        private static string FormatMeetingType(MeetingType meeting)
        {
            return meeting switch
            {
                MeetingType.InPerson => "In-person",
                MeetingType.OnlineMeeting => "Online Meeting",
                MeetingType.PhoneCall => "Phone Call",
                _ => meeting.ToString()
            };
        }

    }
}
