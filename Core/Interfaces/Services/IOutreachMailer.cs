namespace Core.Interfaces.Services
{
    public record OutreachPitchEmail(
        string RecipientEmail,
        string? RecipientName,
        string CompanyName,
        string SenderName,
        byte[] PitchDeckBytes,
        string PitchDeckFileName);

    public interface IOutreachMailer
    {
        /// <summary>
        /// Sends the outreach email (text from OutreachEmailContent) with the
        /// pitch deck attached.
        /// </summary>
        Task SendPitchAsync(OutreachPitchEmail email, CancellationToken cancellationToken = default);
    }
}
