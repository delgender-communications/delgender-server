using Core.DTOs;
using Core.DTOs.Invoice;
using Core.Enums;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DelgenderCommunicationsAPI.Controllers
{
    [ApiController]
    [Route("api/v1/invoices")]
    [Authorize]
    public class InvoicesController : Controller
    {
        private readonly IInvoiceService _invoiceService;

        public InvoicesController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        [HttpPost]
        public async Task<ActionResult<InvoiceDto>> Create([FromBody] CreateInvoiceDto dto)
        {
            var staffId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var invoice = await _invoiceService.CreateAsync(dto, staffId);
            return CreatedAtAction(nameof(GetById), new { id = invoice.Id }, invoice);
        }

        [HttpGet]
        public async Task<ActionResult<PagedResultDto<InvoiceDto>>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] InvoiceStatus? status = null,
            [FromQuery] int? customerId = null)
        {
            var result = await _invoiceService.GetAllAsync(page, pageSize, status, customerId);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<InvoiceDto>> GetById(int id)
        {
            var invoice = await _invoiceService.GetByIdAsync(id);
            return invoice is null ? NotFound() : Ok(invoice);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<InvoiceDto>> Update(int id, [FromBody] UpdateInvoiceDto dto)
        {
            var invoice = await _invoiceService.UpdateAsync(id, dto);
            return invoice is null ? NotFound() : Ok(invoice);
        }

        [HttpPatch("{id:int}/status")]
        public async Task<ActionResult<InvoiceDto>> UpdateStatus(int id, [FromBody] UpdateInvoiceStatusDto dto)
        {
            var invoice = await _invoiceService.UpdateStatusAsync(id, dto);
            return invoice is null ? NotFound() : Ok(invoice);
        }

        [HttpGet("{id:int}/pdf")]
        public async Task<IActionResult> GetPdf(int id)
        {
            var result = await _invoiceService.GeneratePdfAsync(id);
            if (result is null)
            {
                return NotFound();
            }

            return File(result.Value.Bytes, "application/pdf", result.Value.FileName);
        }

        [HttpPost("{id:int}/send")]
        public async Task<ActionResult<InvoiceDto>> Send(int id, [FromBody] SendInvoiceDto dto)
        {
            var invoice = await _invoiceService.SendAsync(id, dto);
            return invoice is null ? NotFound() : Ok(invoice);
        }
    }
}
