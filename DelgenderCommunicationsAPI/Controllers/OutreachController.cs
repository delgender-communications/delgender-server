using Core.DTOs;
using Core.DTOs.Outreach;
using Core.Enums;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DelgenderCommunicationsAPI.Controllers
{
    [ApiController]
    [Route("api/v1/outreach")]
    [Authorize]
    public class OutreachController : Controller
    {
        private static readonly Dictionary<string, string> ReportContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            [".pdf"] = "application/pdf",
            [".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            [".txt"] = "text/plain"
        };

        private readonly IOutreachService _outreachService;

        public OutreachController(IOutreachService outreachService)
        {
            _outreachService = outreachService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResultDto<OutreachListItemDto>>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 15,
            [FromQuery] OutreachStatus? status = null)
        {
            var result = await _outreachService.GetAllAsync(page, pageSize, status);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<OutreachDetailDto>> GetById(int id)
        {
            var company = await _outreachService.GetByIdAsync(id);
            return company is null ? NotFound() : Ok(company);
        }

        [HttpPost]
        public async Task<ActionResult<OutreachDetailDto>> Create([FromBody] CreateOutreachDto dto)
        {
            var company = await _outreachService.CreateAsync(dto, CurrentStaffId());
            return Ok(company);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<OutreachDetailDto>> Update(int id, [FromBody] UpdateOutreachDto dto)
        {
            var company = await _outreachService.UpdateAsync(id, dto, CurrentStaffId());
            return company is null ? NotFound() : Ok(company);
        }

        [HttpPatch("{id:int}/notes")]
        public async Task<ActionResult<OutreachDetailDto>> UpdateNotes(int id, [FromBody] UpdateOutreachNotesDto dto)
        {
            var company = await _outreachService.UpdateNotesAsync(id, dto);
            return company is null ? NotFound() : Ok(company);
        }

        [HttpPost("{id:int}/pitch-deck")]
        [RequestSizeLimit(20_000_000)]
        public async Task<ActionResult<OutreachDetailDto>> UploadPitchDeck(int id, IFormFile file)
        {
            if (file is null || file.Length == 0)
            {
                return BadRequest(new { message = "No pitch deck was uploaded." });
            }

            if (file.Length > 20_000_000)
            {
                return BadRequest(new { message = "That PDF is over 20 MB." });
            }

            var isPdf = string.Equals(file.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase)
                || file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);

            if (!isPdf)
            {
                return BadRequest(new { message = "The pitch deck needs to be a PDF." });
            }

            await using var stream = file.OpenReadStream();
            var company = await _outreachService.UploadPitchDeckAsync(id, stream, file.FileName, CurrentStaffId());
            return company is null ? NotFound() : Ok(company);
        }

        [HttpGet("{id:int}/pitch-deck")]
        public async Task<IActionResult> DownloadPitchDeck(int id)
        {
            var deck = await _outreachService.DownloadPitchDeckAsync(id);
            return deck is null
                ? NotFound()
                : File(deck.Value.Bytes, "application/pdf", deck.Value.FileName);
        }

        [HttpPost("{id:int}/report")]
        [RequestSizeLimit(20_000_000)]
        public async Task<ActionResult<OutreachDetailDto>> UploadReport(int id, IFormFile file)
        {
            if (file is null || file.Length == 0)
            {
                return BadRequest(new { message = "No report was uploaded." });
            }

            if (file.Length > 20_000_000)
            {
                return BadRequest(new { message = "That report is over 20 MB." });
            }

            if (!ReportContentTypes.ContainsKey(Path.GetExtension(file.FileName)))
            {
                return BadRequest(new { message = "The report needs to be a DOCX, TXT or PDF file." });
            }

            await using var stream = file.OpenReadStream();
            var company = await _outreachService.UploadReportAsync(id, stream, file.FileName, CurrentStaffId());
            return company is null ? NotFound() : Ok(company);
        }

        [HttpGet("{id:int}/report")]
        public async Task<IActionResult> DownloadReport(int id)
        {
            var report = await _outreachService.DownloadReportAsync(id);
            if (report is null)
            {
                return NotFound();
            }

            var contentType = ReportContentTypes.TryGetValue(Path.GetExtension(report.Value.FileName), out var type)
                ? type
                : "application/octet-stream";

            return File(report.Value.Bytes, contentType, report.Value.FileName);
        }

        [HttpPost("{id:int}/send-email")]
        public async Task<ActionResult<OutreachDetailDto>> SendEmail(int id)
        {
            var company = await _outreachService.SendEmailAsync(id, CurrentStaffId());
            return company is null ? NotFound() : Ok(company);
        }

        [HttpPost("{id:int}/mark-emailed")]
        public async Task<ActionResult<OutreachDetailDto>> MarkEmailed(int id)
        {
            var company = await _outreachService.MarkEmailedAsync(id, CurrentStaffId());
            return company is null ? NotFound() : Ok(company);
        }

        private int CurrentStaffId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
