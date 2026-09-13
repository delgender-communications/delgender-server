using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Core.Configuration;
using Core.Interfaces.Services;
using Microsoft.Extensions.Options;

namespace Application.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IOptions<CloudinarySettings> options)
        {
            var settings = options.Value;

            if (string.IsNullOrWhiteSpace(settings.CloudName) ||
                string.IsNullOrWhiteSpace(settings.ApiKey) ||
                string.IsNullOrWhiteSpace(settings.ApiSecret))
            {
                throw new InvalidOperationException(
                    "Cloudinary is not configured. Set the Cloudinary:CloudName, ApiKey and ApiSecret settings.");
            }

            var account = new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret);
            _cloudinary = new Cloudinary(account) { Api = { Secure = true } };
        }

        public async Task<(string Url, string PublicId)> UploadImageAsync(Stream fileStream, string fileName, string folder)
        {
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, fileStream),
                Folder = folder,

                Transformation = new Transformation()
                    .Quality("auto").FetchFormat("auto")
                    .Width(400).Height(400).Crop("fill").Gravity("face")
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.Error is not null)
            {
                throw new InvalidOperationException($"Image upload failed: {result.Error.Message}");
            }

            return (result.SecureUrl.ToString(), result.PublicId);
        }

        public async Task DeleteImageAsync(string publicId)
        {
            try
            {
                await _cloudinary.DestroyAsync(new DeletionParams(publicId));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Cloudinary] Failed to delete {publicId}: {ex.Message}");
            }
        }
    }
}
