using Core.Configuration;
using Core.Interfaces.Services;
using Resend;
using System.Net;
using System.Text.RegularExpressions;

namespace Infrastructure.Services
{
    public class OutreachMailer : IOutreachMailer
    {
        private readonly IResend _resend;
        private readonly IEmailTemplateRenderer _renderer;

        public OutreachMailer(IResend resend, IEmailTemplateRenderer renderer)
        {
            _resend = resend;
            _renderer = renderer;
        }

        public async Task SendPitchAsync(OutreachPitchEmail email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(OutreachEmailContent.FromEmail) ||
                string.IsNullOrWhiteSpace(OutreachEmailContent.Subject) ||
                string.IsNullOrWhiteSpace(OutreachEmailContent.Body))
            {
                throw new InvalidOperationException(
                    "The outreach email isn't written yet. Fill in Subject, Heading and Body in OutreachEmailContent.cs.");
            }

            var values = new Dictionary<string, string>
            {
                ["CompanyName"] = email.CompanyName,
                ["ContactName"] = string.IsNullOrWhiteSpace(email.RecipientName) ? $"{email.CompanyName.Trim()} team" : email.RecipientName.Trim(),
                ["SenderName"] = email.SenderName
            };

            var subject = Fill(OutreachEmailContent.Subject, values);
            var heading = Fill(OutreachEmailContent.Heading, values);
            // Strip spaces at the end of each line, so a "blank" line that still holds
            // a few spaces (easy to leave behind in an editor) is treated as blank and
            // still starts a new paragraph.
            var text = Regex.Replace(
                Fill(OutreachEmailContent.Body, values).Replace("\r\n", "\n"),
                @"[ \t]+\n",
                "\n").Trim();

            var layoutTokens = new Dictionary<string, string?>
            {
                ["Heading"] = string.IsNullOrWhiteSpace(heading) ? subject : heading,
                ["Intro"] = string.Empty,
                ["BodyHtml"] = ToHtml(text),
                ["FooterNote"] = OutreachEmailContent.FooterNote,
                ["Year"] = DateTime.UtcNow.Year.ToString()
            };

            var message = new EmailMessage
            {
                From = new EmailAddress
                {
                    Email = OutreachEmailContent.FromEmail,
                    DisplayName = OutreachEmailContent.FromName
                },
                Subject = subject,
                HtmlBody = _renderer.Render("Layout.html", layoutTokens),
                TextBody = text,
                Attachments = new List<EmailAttachment>
                {
                    new EmailAttachment
                    {
                        Filename = email.PitchDeckFileName,
                        Content = email.PitchDeckBytes
                    }
                }
            };

            message.To.Add(email.RecipientEmail);

            if (!string.IsNullOrWhiteSpace(OutreachEmailContent.ReplyTo))
            {
                message.ReplyTo = OutreachEmailContent.ReplyTo;
            }

            await _resend.EmailSendAsync(message, cancellationToken);
        }

        private static string Fill(string template, IReadOnlyDictionary<string, string> values)
        {
            var result = template;
            foreach (var pair in values)
            {
                result = result.Replace("{{" + pair.Key + "}}", pair.Value);
            }

            return result;
        }

        // Same paragraph styling as the other emails. Blank line = new paragraph,
        // a single line break inside a paragraph is kept.
        private static string ToHtml(string text) =>
            string.Join(string.Empty, text
                .Split("\n\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(paragraph =>
                    "<p style=\"margin:0 0 14px;font-size:15px;line-height:24px;color:#374151;\">"
                    + WebUtility.HtmlEncode(paragraph).Replace("\n", "<br>")
                    + "</p>"));
    }
}
