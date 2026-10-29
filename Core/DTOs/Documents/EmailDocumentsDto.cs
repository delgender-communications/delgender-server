using System.ComponentModel.DataAnnotations;

namespace Core.DTOs.Documents
{
    public class EmailDocumentsDto
    {
        [Required]
        public int ClientId { get; set; }

        [Required, MinLength(1, ErrorMessage = "Tick at least one document to send.")]
        public List<int> DocumentIds { get; set; } = new();

        [Required, StringLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required, StringLength(8000)]
        public string Body { get; set; } = string.Empty;
    }
}
