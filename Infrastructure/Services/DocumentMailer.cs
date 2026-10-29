using Core.Configuration;
using Core.Interfaces.Services;
using Resend;
using System.Net;
using System.Text.RegularExpressions;

namespace Infrastructure.Services
{
    public class DocumentMailer : IDocumentMailer
    {
        private readonly IResend _resend;
        private readonly IEmailTemplateRenderer _renderer;

        public DocumentMailer(IResend resend, IEmailTemplateRenderer renderer)
        {
            _resend = resend;
            _renderer = renderer;
        }

        public async Task SendAsync(DocumentsEmail email, CancellationToken cancellationToken = default)
        {
            var subject = email.Subject.Trim();

            // Strip trailing spaces on each line so a "blank" line that still holds a few
            // spaces still starts a new paragraph.
            var text = Regex.Replace(
                email.Body.Replace("\r\n", "\n"),
                @"[ \t]+\n",
                "\n").Trim();

            var layoutTokens = new Dictionary<string, string?>
            {
                ["Heading"] = subject,
                ["Intro"] = string.Empty,
                ["BodyHtml"] = ToHtml(text),
                ["FooterNote"] = DocumentEmailContent.FooterNote,
                ["Year"] = DateTime.UtcNow.Year.ToString()
            };

            var message = new EmailMessage
            {
                From = new EmailAddress
                {
                    Email = DocumentEmailContent.FromEmail,
                    DisplayName = DocumentEmailContent.FromName
                },
                Subject = subject,
                HtmlBody = _renderer.Render("Layout.html", layoutTokens),
                TextBody = text,
                Attachments = email.Attachments
                    .Select(a => new EmailAttachment { Filename = a.FileName, Content = a.Bytes })
                    .ToList()
            };

            message.To.Add(email.RecipientEmail);

            if (!string.IsNullOrWhiteSpace(DocumentEmailContent.ReplyTo))
            {
                message.ReplyTo = DocumentEmailContent.ReplyTo;
            }

            await _resend.EmailSendAsync(message, cancellationToken);
        }

        // Blank line = new paragraph, a single line break inside a paragraph is kept.
        private static string ToHtml(string text) =>
            string.Join(string.Empty, text
                .Split("\n\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(paragraph =>
                    "<p style=\"margin:0 0 14px;font-size:15px;line-height:24px;color:#374151;\">"
                    + WebUtility.HtmlEncode(paragraph).Replace("\n", "<br>")
                    + "</p>"));
    }
}
