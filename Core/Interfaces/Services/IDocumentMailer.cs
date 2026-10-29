namespace Core.Interfaces.Services
{
    public record DocumentAttachment(string FileName, byte[] Bytes);

    public record DocumentsEmail(
        string RecipientEmail,
        string? RecipientName,
        string Subject,
        string Body,
        IReadOnlyList<DocumentAttachment> Attachments);

    public interface IDocumentMailer
    {
        Task SendAsync(DocumentsEmail email, CancellationToken cancellationToken = default);
    }
}
