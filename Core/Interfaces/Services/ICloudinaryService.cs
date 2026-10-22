namespace Core.Interfaces.Services
{
    public interface ICloudinaryService
    {
        Task<(string Url, string PublicId)> UploadImageAsync(Stream fileStream, string fileName, string folder);
        Task DeleteImageAsync(string publicId);
        Task<(string Url, string PublicId)> UploadRawAsync(Stream fileStream, string fileName, string folder);
        Task DeleteRawAsync(string publicId);

        /// <summary>
        /// Short-lived signed link for a raw file. Works even when the account blocks
        /// public delivery of PDFs, which a plain secure URL does not.
        /// </summary>
        string GetRawDownloadUrl(string publicId);
    }
}
