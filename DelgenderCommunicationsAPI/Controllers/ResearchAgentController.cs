using Core.DTOs.ResearchLeads;
using Core.Interfaces.Services;
using DelgenderCommunicationsAPI.Filters;
using Microsoft.AspNetCore.Mvc;

namespace DelgenderCommunicationsAPI.Controllers
{
    [ApiController]
    [Route("api/v1/research-agent")]
    [ServiceFilter(typeof(AgentApiKeyFilter))]
    public class ResearchAgentController : Controller
    {
        private readonly IResearchLeadService _researchLeadService;

        public ResearchAgentController(IResearchLeadService researchLeadService)
        {
            _researchLeadService = researchLeadService;
        }

        [HttpPost("leads")]
        public async Task<ActionResult<IngestResultDto>> Ingest([FromBody] IngestResearchLeadsDto dto)
        {
            var result = await _researchLeadService.IngestAsync(dto);
            return Ok(result);
        }
    }
}
