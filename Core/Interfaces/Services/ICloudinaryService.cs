namespace Core.Interfaces.Services
{
    public interface ICloudinaryService
    {
        Task<(string Url, string PublicId)> UploadImageAsync(Stream fileStream, string fileName, string folder);
        Task DeleteImageAsync(string publicId);
        Task<(string Url, string PublicId)> UploadRawAsync(Stream fileStream, string fileName, string folder);
        Task DeleteRawAsync(string publicId);
    }
}
