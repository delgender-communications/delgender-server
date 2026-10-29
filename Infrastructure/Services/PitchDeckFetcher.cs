using Core.Interfaces.Services;
using System.Net;

namespace Infrastructure.Services
{
    public class PitchDeckFetcher : IPitchDeckFetcher
    {
        // One shared client for the app's lifetime.
        private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(60) };

        public async Task<byte[]> FetchAsync(string url, CancellationToken cancellationToken = default)
        {
            try
            {
                using var response = await Http.GetAsync(url, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsByteArrayAsync(cancellationToken);
                }

                if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                {
                    throw new InvalidOperationException(
                        "The file host refused to hand over this file. If you use Cloudinary, check that PDF delivery is allowed for the account, then try again.");
                }

                throw new InvalidOperationException(
                    $"Couldn't fetch the file (the file host answered {(int)response.StatusCode}). Try uploading it again.");
            }
            catch (HttpRequestException)
            {
                throw new InvalidOperationException("Couldn't reach the file host to get the file. Try again in a moment.");
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                throw new InvalidOperationException("Getting the file took too long. Try again in a moment.");
            }
        }
    }
}
