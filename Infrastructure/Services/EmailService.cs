using Core.DTOs;
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
