using Core.DTOs;
using Core.DTOs.Client;
using Core.Enums;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DelgenderCommunicationsAPI.Controllers
{
    [ApiController]
    [Route("api/v1/clients")]
    [Authorize]
    public class ClientsController : Controller
    {
        private readonly IClientService _clientService;

        public ClientsController(IClientService clientService)
        {
            _clientService = clientService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResultDto<ClientDto>>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 15,
            [FromQuery] ClientStatus? status = null)
        {
            var result = await _clientService.GetAllAsync(page, pageSize, status);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<ClientDto>> Create([FromBody] CreateClientDto dto)
        {
            var client = await _clientService.CreateAsync(dto);
            return Ok(client);
        }

        [HttpPatch("{id:int}/status")]
        public async Task<ActionResult<ClientDto>> UpdateStatus(int id, [FromBody] UpdateClientStatusDto dto)
        {
            var staffId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var client = await _clientService.UpdateStatusAsync(id, dto, staffId);
            return client is null ? NotFound() : Ok(client);
        }
    }
}
