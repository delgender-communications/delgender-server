namespace Core.Interfaces.Services
{
    public interface IPitchDeckFetcher
    {
        /// <summary>
        /// Downloads a stored pitch deck server-side, so the browser never has to
        /// reach the file host directly. Throws InvalidOperationException with a
        /// readable message when the file can't be retrieved.
        /// </summary>
        Task<byte[]> FetchAsync(string url, CancellationToken cancellationToken = default);
    }
}
