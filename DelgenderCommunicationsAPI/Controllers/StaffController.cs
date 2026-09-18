using Core.DTOs;
using Core.DTOs.Auth;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DelgenderCommunicationsAPI.Controllers
{
    [ApiController]
    [Route("api/v1/staff")]
    [Authorize]
    public class StaffController : Controller
    {
        private readonly IStaffService _staffService;

        public StaffController(IStaffService staffService)
        {
            _staffService = staffService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<StaffDto>> Create([FromBody] CreateStaffDto dto)
        {
            var staff = await _staffService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = staff.Id }, staff);
        }

        [HttpGet]
        public async Task<ActionResult<PagedResultDto<StaffDto>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            // Non-admins get a directory view: name, job title, staff ID, avatar only.
            // The sensitive fields (role, active status, last login, email, phone) are
            // stripped in the service rather than just hidden in the UI.
            var isAdmin = User.IsInRole("Admin");
            var result = await _staffService.GetAllAsync(page, pageSize, isAdmin);
            return Ok(result);
        }

        [HttpGet("me")]
        public async Task<ActionResult<StaffDto>> GetMe()
        {
            var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var staff = await _staffService.GetByIdAsync(id);
            return staff is null ? NotFound() : Ok(staff);
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<StaffDto>> GetById(int id)
        {
            var staff = await _staffService.GetByIdAsync(id);
            return staff is null ? NotFound() : Ok(staff);
        }

        [HttpPatch("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<StaffDto>> Update(int id, [FromBody] UpdateStaffDto dto)
        {
            var staff = await _staffService.UpdateAsync(id, dto);
            return staff is null ? NotFound() : Ok(staff);
        }

        [HttpPost("me/profile-picture")]
        [RequestSizeLimit(5_000_000)]
        public async Task<ActionResult<StaffDto>> UploadMyProfilePicture(IFormFile file)
        {
            if (file is null || file.Length == 0)
            {
                return BadRequest(new { message = "No file was uploaded." });
            }
            if (!file.ContentType.StartsWith("image/"))
            {
                return BadRequest(new { message = "Only image files are allowed." });
            }

            var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await using var stream = file.OpenReadStream();
            var staff = await _staffService.UpdateProfilePictureAsync(id, stream, file.FileName);
            return staff is null ? NotFound() : Ok(staff);
        }

        [HttpPost("{id:int}/profile-picture")]
        [Authorize(Roles = "Admin")]
        [RequestSizeLimit(5_000_000)]
        public async Task<ActionResult<StaffDto>> UploadProfilePicture(int id, IFormFile file)
        {
            if (file is null || file.Length == 0)
            {
                return BadRequest(new { message = "No file was uploaded." });
            }
            if (!file.ContentType.StartsWith("image/"))
            {
                return BadRequest(new { message = "Only image files are allowed." });
            }

            await using var stream = file.OpenReadStream();
            var staff = await _staffService.UpdateProfilePictureAsync(id, stream, file.FileName);
            return staff is null ? NotFound() : Ok(staff);
        }
    }
}
