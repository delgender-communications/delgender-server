using Core.DTOs;
using Core.DTOs.ResearchLeads;
using Core.Enums;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DelgenderCommunicationsAPI.Controllers
{
    [ApiController]
    [Route("api/v1/research-leads")]
    [Authorize]
    public class ResearchLeadsController : Controller
    {
        private readonly IResearchLeadService _researchLeadService;

        public ResearchLeadsController(IResearchLeadService researchLeadService)
        {
            _researchLeadService = researchLeadService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResultDto<ResearchLeadDto>>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 15,
            [FromQuery] ResearchLeadStatus? status = null,
            [FromQuery(Name = "case")] ResearchLeadCase? leadCase = null)
        {
            var result = await _researchLeadService.GetAllAsync(page, pageSize, status, leadCase);
            return Ok(result);
        }

        [HttpGet("new-count")]
        public async Task<ActionResult> GetNewCount()
        {
            var count = await _researchLeadService.GetNewCountAsync();
            return Ok(new { count });
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ResearchLeadDto>> GetById(int id)
        {
            var lead = await _researchLeadService.GetByIdAsync(id);
            return lead is null ? NotFound() : Ok(lead);
        }

        [HttpPost]
        public async Task<ActionResult<ResearchLeadDto>> AddManual([FromBody] ResearchLeadInputDto dto)
        {
            var lead = await _researchLeadService.AddManualAsync(dto, CurrentStaffId());
            return Ok(lead);
        }

        [HttpPost("{id:int}/accept")]
        public async Task<ActionResult<ResearchLeadDto>> Accept(int id)
        {
            var lead = await _researchLeadService.AcceptAsync(id, CurrentStaffId());
            return lead is null ? NotFound() : Ok(lead);
        }

        [HttpPost("{id:int}/dismiss")]
        public async Task<ActionResult<ResearchLeadDto>> Dismiss(int id, [FromBody] DismissResearchLeadDto dto)
        {
            var lead = await _researchLeadService.DismissAsync(id, dto, CurrentStaffId());
            return lead is null ? NotFound() : Ok(lead);
        }

        private int CurrentStaffId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
