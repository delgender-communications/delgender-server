namespace Core.Interfaces.Services
{
    public interface ICloudinaryService
    {
        Task<(string Url, string PublicId)> UploadImageAsync(Stream fileStream, string fileName, string folder);
        Task DeleteImageAsync(string publicId);
    }
}
