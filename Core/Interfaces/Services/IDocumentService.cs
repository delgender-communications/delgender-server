using Core.DTOs.Documents;

namespace Core.Interfaces.Services
{
    public interface IDocumentService
    {
        Task<List<DocumentDto>> GetAllAsync();
        Task<DocumentDto> UploadAsync(string? title, string? description, Stream fileStream, string fileName, long sizeBytes, int staffId);
        Task<(byte[] Bytes, string FileName)?> DownloadAsync(int id);
        Task<bool> DeleteAsync(int id, int staffId, bool isAdmin);
        Task<DocumentEmailTemplateDto?> GetEmailTemplateAsync(int clientId, int staffId);
        Task<DocumentsSentDto?> SendEmailAsync(EmailDocumentsDto dto, int staffId);
    }
}
