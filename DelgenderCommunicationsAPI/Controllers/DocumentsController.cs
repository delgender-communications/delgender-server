using Core.DTOs.Documents;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DelgenderCommunicationsAPI.Controllers
{
    [ApiController]
    [Route("api/v1/documents")]
    [Authorize]
    public class DocumentsController : Controller
    {
        private const long MaxFileBytes = 20_000_000;

        private static readonly Dictionary<string, string> ContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            [".pdf"] = "application/pdf",
            [".doc"] = "application/msword",
            [".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            [".xls"] = "application/vnd.ms-excel",
            [".xlsx"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            [".ppt"] = "application/vnd.ms-powerpoint",
            [".pptx"] = "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            [".txt"] = "text/plain",
            [".png"] = "image/png",
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg"
        };

        private readonly IDocumentService _documentService;

        public DocumentsController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        [HttpGet]
        public async Task<ActionResult<List<DocumentDto>>> GetAll()
        {
            return Ok(await _documentService.GetAllAsync());
        }

        [HttpPost]
        [RequestSizeLimit(21_000_000)]
        public async Task<ActionResult<DocumentDto>> Upload(
            [FromForm] string? title,
            [FromForm] string? description,
            IFormFile file)
        {
            if (file is null || file.Length == 0)
            {
                return BadRequest(new { message = "No file was uploaded." });
            }

            if (file.Length > MaxFileBytes)
            {
                return BadRequest(new { message = "That file is over 20 MB." });
            }

            if (!ContentTypes.ContainsKey(Path.GetExtension(file.FileName)))
            {
                return BadRequest(new { message = "That file type isn't supported. Use PDF, Word, Excel, PowerPoint, TXT, PNG or JPG." });
            }

            if (title is { Length: > 150 })
            {
                return BadRequest(new { message = "The title is too long. Keep it under 150 characters." });
            }

            if (description is { Length: > 500 })
            {
                return BadRequest(new { message = "The description is too long. Keep it under 500 characters." });
            }

            await using var stream = file.OpenReadStream();
            var document = await _documentService.UploadAsync(
                title, description, stream, file.FileName, file.Length, CurrentStaffId());
            return Ok(document);
        }

        [HttpGet("{id:int}/download")]
        public async Task<IActionResult> Download(int id)
        {
            var document = await _documentService.DownloadAsync(id);
            if (document is null)
            {
                return NotFound();
            }

            var contentType = ContentTypes.TryGetValue(Path.GetExtension(document.Value.FileName), out var type)
                ? type
                : "application/octet-stream";

            return File(document.Value.Bytes, contentType, document.Value.FileName);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _documentService.DeleteAsync(id, CurrentStaffId(), User.IsInRole("Admin"));
            return deleted ? NoContent() : NotFound();
        }

        [HttpGet("email-template")]
        public async Task<ActionResult<DocumentEmailTemplateDto>> GetEmailTemplate([FromQuery] int clientId)
        {
            var template = await _documentService.GetEmailTemplateAsync(clientId, CurrentStaffId());
            return template is null ? NotFound() : Ok(template);
        }

        [HttpPost("send-email")]
        public async Task<ActionResult<DocumentsSentDto>> SendEmail([FromBody] EmailDocumentsDto dto)
        {
            var result = await _documentService.SendEmailAsync(dto, CurrentStaffId());
            return result is null ? NotFound() : Ok(result);
        }

        private int CurrentStaffId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
